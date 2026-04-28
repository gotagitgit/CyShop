namespace Basket.Domain.Entities;

public sealed record BasketItem
{
    public Guid ProductId { get; init; }
    public string ProductName { get; init; } = string.Empty;
    public decimal UnitPrice { get; init; }
    public decimal OldUnitPrice { get; init; }
    public int Quantity { get; init; }
    public string PictureUrl { get; init; } = string.Empty;
}
