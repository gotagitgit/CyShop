using EventBus.Abstractions;
using EventBus.Events;
using Microsoft.Extensions.Logging;
using Orders.Application.IntegrationEvents;
using Orders.Application.Interfaces;

namespace Orders.Application.Services;

public class OrderingIntegrationEventService(
    IIntegrationEventLogRepository eventLogRepository,
    IEventBus eventBus,
    ILogger<OrderingIntegrationEventService> logger) : IIntegrationEventService
{
    private static readonly Dictionary<string, Type> EventTypes = new()
    {
        [typeof(IntegrationEvents.Events.OrderStartedIntegrationEvent).FullName!] =
            typeof(IntegrationEvents.Events.OrderStartedIntegrationEvent)
    };

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

                if (!EventTypes.TryGetValue(entry.EventTypeName, out var eventType))
                {
                    logger.LogWarning("Unknown event type {EventType}, skipping", entry.EventTypeName);
                    await eventLogRepository.MarkEventAsFailedAsync(entry.EventId, ct);
                    continue;
                }

                var integrationEvent = entry.DeserializeEvent(eventType);
                await eventBus.PublishAsync(integrationEvent);

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
