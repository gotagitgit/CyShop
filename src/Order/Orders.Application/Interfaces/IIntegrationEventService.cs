using EventBus.Events;

namespace Orders.Application.Interfaces;

public interface IIntegrationEventService
{
    Task AddAndSaveEventAsync(IntegrationEvent evt, CancellationToken ct = default);

    Task PublishPendingEventsAsync(CancellationToken ct = default);
}
