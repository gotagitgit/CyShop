using System.Text;
using EventBus.Abstractions;
using EventBus.Events;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace RabbitMQEventBus;

public abstract class RabbitMQEventBusBackgroundService(
    IServiceProvider serviceProvider,
    IOptions<EventBusOptions> options,
    IOptions<EventBusSubscriptionInfo> subscriptionOptions,
    ILogger logger) : BackgroundService, IDisposable
{
    protected const string ExchangeName = "cyshop_event_bus";

    private string _queueName = options.Value.SubscriptionClientName;
    private IChannel? _consumerChannel;

    protected EventBusSubscriptionInfo SubscriptionInfo { get; } = subscriptionOptions.Value;

    protected IConnection? Connection { get; private set; }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            logger.LogInformation("Starting RabbitMQ connection");

            var factory = serviceProvider.GetRequiredService<IConnectionFactory>();
            Connection = await factory.CreateConnectionAsync(stoppingToken);

            if (!Connection.IsOpen)
            {
                logger.LogWarning("RabbitMQ connection is not open");
                return;
            }

            if (logger.IsEnabled(LogLevel.Trace))
            {
                logger.LogTrace("Creating RabbitMQ consumer channel");
            }

            _consumerChannel = await Connection.CreateChannelAsync(cancellationToken: stoppingToken);

            _consumerChannel.CallbackExceptionAsync += (sender, ea) =>
            {
                logger.LogWarning(ea.Exception, "Error with RabbitMQ consumer channel");
                return Task.CompletedTask;
            };

            await _consumerChannel.ExchangeDeclareAsync(
                exchange: ExchangeName, type: "direct", cancellationToken: stoppingToken);

            await _consumerChannel.QueueDeclareAsync(
                queue: _queueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null,
                cancellationToken: stoppingToken);

            if (logger.IsEnabled(LogLevel.Trace))
            {
                logger.LogTrace("Starting RabbitMQ basic consume");
            }

            var consumer = new AsyncEventingBasicConsumer(_consumerChannel);
            consumer.ReceivedAsync += OnMessageReceived;

            await _consumerChannel.BasicConsumeAsync(
                queue: _queueName,
                autoAck: false,
                consumer: consumer,
                cancellationToken: stoppingToken);

            foreach (var (eventName, _) in SubscriptionInfo.EventTypes)
            {
                await _consumerChannel.QueueBindAsync(
                    queue: _queueName,
                    exchange: ExchangeName,
                    routingKey: eventName,
                    cancellationToken: stoppingToken);
            }

            logger.LogInformation("RabbitMQ consumer started on queue '{QueueName}'", _queueName);
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            // Graceful shutdown
            logger.LogInformation("RabbitMQ consumer is stopping due to cancellation");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error starting RabbitMQ connection");
        }
    }

    private async Task OnMessageReceived(object sender, BasicDeliverEventArgs eventArgs)
    {
        var eventName = eventArgs.RoutingKey;
        var message = Encoding.UTF8.GetString(eventArgs.Body.Span);

        try
        {
            if (logger.IsEnabled(LogLevel.Trace))
            {
                logger.LogTrace("Processing RabbitMQ event: {EventName}", eventName);
            }

            await ProcessEvent(eventName, message);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Error processing message \"{Message}\"", message);
        }

        // Even on exception we take the message off the queue.
        // In a real-world app this should be handled with a Dead Letter Exchange (DLX).
        // See: https://www.rabbitmq.com/dlx.html
        await _consumerChannel!.BasicAckAsync(eventArgs.DeliveryTag, multiple: false);
    }

    private async Task ProcessEvent(string eventName, string message)
    {
        await using var scope = serviceProvider.CreateAsyncScope();

        if (!SubscriptionInfo.EventTypes.TryGetValue(eventName, out var eventType))
        {
            logger.LogWarning("Unable to resolve event type for event name {EventName}", eventName);
            return;
        }

        var integrationEvent = DeserializeMessage(message, eventType);

        foreach (var handler in scope.ServiceProvider.GetKeyedServices<IIntegrationEventHandler>(eventType))
        {
            await handler.Handle(integrationEvent);
        }
    }

    protected abstract IntegrationEvent DeserializeMessage(string message, Type eventType);

    public override void Dispose()
    {
        _consumerChannel?.Dispose();
        Connection?.Dispose();
        base.Dispose();
    }
}
