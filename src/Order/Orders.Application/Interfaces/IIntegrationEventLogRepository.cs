using EventBus.Events;
using Orders.Application.IntegrationEvents;

namespace Orders.Application.Interfaces;

public interface IIntegrationEventLogRepository
{
    Task SaveEventAsync(IntegrationEvent evt, CancellationToken ct = default);
    Task<IReadOnlyList<IntegrationEventLogEntry>> GetPendingEventsAsync(CancellationToken ct = default);
    Task MarkEventAsInProgressAsync(Guid eventId, CancellationToken ct = default);
    Task MarkEventAsPublishedAsync(Guid eventId, CancellationToken ct = default);
    Task MarkEventAsFailedAsync(Guid eventId, CancellationToken ct = default);
}
