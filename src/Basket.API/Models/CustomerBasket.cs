namespace Basket.API.Models;

public sealed record CustomerBasket
{
    public string BuyerId { get; init; } = string.Empty;
    public List<BasketItem> Items { get; init; } = [];
}
