namespace Orders.Application.Interfaces;

public interface IIdempotencyRepository
{
    Task<bool> ExistsAsync(Guid idempotencyKey, CancellationToken ct = default);
    Task AddAsync(Guid idempotencyKey, CancellationToken ct = default);
}
