using Microsoft.EntityFrameworkCore;
using Orders.Application.Interfaces;
using Orders.Infrastructure.Data;
using Orders.Infrastructure.Data.Entities;

namespace Orders.Infrastructure.Repositories;

public class IdempotencyRepository(OrdersDbContext context) : IIdempotencyRepository
{
    public async Task<bool> ExistsAsync(Guid idempotencyKey, CancellationToken ct = default)
    {
        return await context.IdempotencyRecords
            .AnyAsync(r => r.IdempotencyKey == idempotencyKey, ct);
    }

    public async Task AddAsync(Guid idempotencyKey, CancellationToken ct = default)
    {
        context.IdempotencyRecords.Add(new IdempotencyRecord
        {
            IdempotencyKey = idempotencyKey,
            Timestamp = DateTime.UtcNow
        });
        await context.SaveChangesAsync(ct);
    }
}
