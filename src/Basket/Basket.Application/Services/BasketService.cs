using Basket.Application.DTOs;
using Basket.Application.Interfaces;
using Basket.Domain.Entities;
using Basket.Domain.Interfaces;
using Cyshop.Common.Models;

namespace Basket.Application.Services;

public class BasketService(IBasketRepository repository, ICurrentUser currentUser) : IBasketService
{
    public async Task<BasketResponseDto> GetBasketAsync(CancellationToken ct = default)
    {
        var basket = await repository.GetBasketAsync(currentUser.UserId);
        if (basket is null)
            return new BasketResponseDto([]);

        return MapToResponse(basket);
    }

    public async Task<BasketResponseDto> UpdateBasketAsync(UpdateBasketDto dto, CancellationToken ct = default)
    {
        if (dto.Items.Any(i => i.Quantity < 1))
            throw new ArgumentException("All item quantities must be at least 1.");

        var basket = new CustomerBasket
        {
            BuyerId = currentUser.UserId,
            Items = dto.Items.Select(MapToEntity).ToList()
        };

        var updated = await repository.UpdateBasketAsync(basket);
        return updated is null ? new BasketResponseDto([]) : MapToResponse(updated);
    }

    public async Task DeleteBasketAsync(CancellationToken ct = default)
    {
        await repository.DeleteBasketAsync(currentUser.UserId);
    }

    public async Task DeleteBasketAsync(Guid buyerId, CancellationToken ct = default)
    {
        await repository.DeleteBasketAsync(buyerId);
    }

    private static BasketResponseDto MapToResponse(CustomerBasket basket) =>
        new(basket.Items.Select(i => new BasketItemDto(
            i.ProductId,
            i.ProductName,
            i.UnitPrice,
            i.OldUnitPrice,
            i.Quantity,
            i.PictureUrl)).ToList());

    private static BasketItem MapToEntity(BasketItemDto dto) =>
        new()
        {
            ProductId = dto.ProductId,
            ProductName = dto.ProductName,
            UnitPrice = dto.UnitPrice,
            OldUnitPrice = dto.OldUnitPrice,
            Quantity = dto.Quantity,
            PictureUrl = dto.PictureUrl
        };
}
