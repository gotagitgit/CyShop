using Microsoft.Extensions.Logging;
using Orders.Application.IntegrationEvents;
using Orders.Application.Interfaces;

namespace Orders.Application.Services;

public class OrderingIntegrationEventService(
    IIntegrationEventLogRepository eventLogRepository,
    ILogger<OrderingIntegrationEventService> logger) : IIntegrationEventService
{
    public async Task AddAndSaveEventAsync(IntegrationEvent evt, CancellationToken ct = default)
    {
        logger.LogInformation("Saving integration event {EventId} ({EventType}) to outbox",
            evt.Id, evt.EventTypeName);

        await eventLogRepository.SaveEventAsync(evt, ct);
    }

    public async Task PublishPendingEventsAsync(CancellationToken ct = default)
    {
        var pendingEvents = await eventLogRepository.GetPendingEventsAsync(ct);

        foreach (var entry in pendingEvents)
        {
            try
            {
                await eventLogRepository.MarkEventAsInProgressAsync(entry.EventId, ct);

                logger.LogInformation("Publishing integration event {EventId} ({EventType})",
                    entry.EventId, entry.EventTypeName);

                // TODO: Publish to your message broker here (RabbitMQ, Azure Service Bus, etc.)
                // await _messageBroker.PublishAsync(entry.DeserializeEvent(eventType), ct);

                await eventLogRepository.MarkEventAsPublishedAsync(entry.EventId, ct);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to publish integration event {EventId}", entry.EventId);
                await eventLogRepository.MarkEventAsFailedAsync(entry.EventId, ct);
            }
        }
    }
}
