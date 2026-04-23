using Orders.Application.Interfaces;

namespace Orders.Application.Services;

public class IdempotencyService(IIdempotencyRepository repository) : IIdempotencyService
{
    public async Task<bool> IsDuplicateAsync(Guid idempotencyKey, CancellationToken ct = default)
    {
        return await repository.ExistsAsync(idempotencyKey, ct);
    }

    public async Task RecordAsync(Guid idempotencyKey, CancellationToken ct = default)
    {
        await repository.AddAsync(idempotencyKey, ct);
    }
}
