namespace Basket.Application.DTOs;

public sealed record BasketItemDto(
    Guid ProductId,
    string ProductName,
    decimal UnitPrice,
    decimal OldUnitPrice,
    int Quantity,
    string PictureUrl);
