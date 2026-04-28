namespace Basket.Domain.Entities;

public sealed record CustomerBasket
{
    public Guid BuyerId { get; init; }
    public List<BasketItem> Items { get; init; } = [];
}
