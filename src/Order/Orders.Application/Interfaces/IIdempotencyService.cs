namespace Orders.Application.Interfaces;

public interface IIdempotencyService
{
    Task<bool> IsDuplicateAsync(Guid idempotencyKey, CancellationToken ct = default);
    Task RecordAsync(Guid idempotencyKey, CancellationToken ct = default);
}
