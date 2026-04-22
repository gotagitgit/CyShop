using Microsoft.EntityFrameworkCore;
using Orders.Infrastructure.Data;

namespace Orders.API.Middleware;

public static class IdempotencyMiddleware
{
    public static async Task<bool> IsDuplicateAsync(Guid idempotencyKey, OrdersDbContext dbContext)
    {
        return await dbContext.IdempotencyRecords
            .AnyAsync(r => r.IdempotencyKey == idempotencyKey);
    }
}
