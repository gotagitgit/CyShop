using EventBus.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;

namespace RabbitMQEventBus;

public static class RabbitMqDependencyInjectionExtensions
{
    private const string SectionName = "EventBus";

    public static IEventBusBuilder AddRabbitMqEventBus(
        this IHostApplicationBuilder builder, string connectionName)
    {
        ArgumentNullException.ThrowIfNull(builder);

        // Register RabbitMQ connection factory (connection created async in StartAsync)
        builder.Services.AddSingleton<IConnectionFactory>(sp =>
        {
            var connectionString = builder.Configuration.GetConnectionString(connectionName)
                ?? throw new InvalidOperationException(
                    $"Connection string '{connectionName}' not found.");

            return new ConnectionFactory { Uri = new Uri(connectionString) };
        });

        // Options support
        builder.Services.Configure<EventBusOptions>(builder.Configuration.GetSection(SectionName));

        // Register as concrete singleton, then alias both interfaces to the same instance
        builder.Services.AddSingleton<RabbitMQEventBus>();
        builder.Services.AddSingleton<IEventBus>(sp => sp.GetRequiredService<RabbitMQEventBus>());
        builder.Services.AddSingleton<IHostedService>(sp => sp.GetRequiredService<RabbitMQEventBus>());

        return new EventBusBuilder(builder.Services);
    }

    private class EventBusBuilder(IServiceCollection services) : IEventBusBuilder
    {
        public IServiceCollection Services => services;
    }
}
