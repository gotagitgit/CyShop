using Orders.Domain.Enums;
using Orders.Domain.ValueObjects;

namespace Orders.Domain.Entities;

public sealed record Order
{
    public Guid Id { get; init; }
    public Guid CustomerId { get; init; }
    public string CustomerName { get; init; } = string.Empty;
    public DateTime OrderDate { get; init; }
    public OrderStatus Status { get; init; }
    public IReadOnlyList<OrderItem> Items { get; init; } = [];
    public ShippingAddress ShippingAddress { get; init; } = null!;
}
