using EventBus.Events;
using Microsoft.EntityFrameworkCore;
using Orders.Application.IntegrationEvents;
using Orders.Application.Interfaces;
using Orders.Infrastructure.Data;
using System.Text.Json;

namespace Orders.Infrastructure.Repositories;

public class IntegrationEventLogRepository(OrdersDbContext context) : IIntegrationEventLogRepository
{
    public async Task SaveEventAsync(IntegrationEvent evt, CancellationToken ct = default)
    {
        var entry = new IntegrationEventLogEntry
        {
            EventId = evt.Id,
            EventTypeName = evt.EventTypeName,
            Content = JsonSerializer.Serialize(evt, evt.GetType()),
            State = EventState.NotPublished,
            CreationTime = evt.CreationDate,
            TimesSent = 0
        };

        context.IntegrationEventLogs.Add(entry);
        await context.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<IntegrationEventLogEntry>> GetPendingEventsAsync(CancellationToken ct = default)
    {
        return await context.IntegrationEventLogs
            .Where(e => e.State == EventState.NotPublished)
            .OrderBy(e => e.CreationTime)
            .ToListAsync(ct);
    }

    public async Task MarkEventAsInProgressAsync(Guid eventId, CancellationToken ct = default)
    {
        await UpdateEventStateAsync(eventId, EventState.InProgress, ct);
    }

    public async Task MarkEventAsPublishedAsync(Guid eventId, CancellationToken ct = default)
    {
        await UpdateEventStateAsync(eventId, EventState.Published, ct);
    }

    public async Task MarkEventAsFailedAsync(Guid eventId, CancellationToken ct = default)
    {
        await UpdateEventStateAsync(eventId, EventState.PublishedFailed, ct);
    }

    private async Task UpdateEventStateAsync(Guid eventId, EventState state, CancellationToken ct)
    {
        var entry = await context.IntegrationEventLogs.FindAsync([eventId], ct)
            ?? throw new InvalidOperationException($"Integration event {eventId} not found.");

        entry.State = state;
        if (state == EventState.InProgress)
            entry.TimesSent++;

        await context.SaveChangesAsync(ct);
    }
}
