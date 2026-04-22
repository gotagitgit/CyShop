namespace Orders.Infrastructure.Data.Entities;

public class IdempotencyRecord
{
    public Guid IdempotencyKey { get; set; }
    public DateTime Timestamp { get; set; }
}
