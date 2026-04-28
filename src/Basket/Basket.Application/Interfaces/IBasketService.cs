using Basket.Application.DTOs;

namespace Basket.Application.Interfaces;

public interface IBasketService
{
    Task<BasketResponseDto> GetBasketAsync(CancellationToken ct = default);
    Task<BasketResponseDto> UpdateBasketAsync(UpdateBasketDto dto, CancellationToken ct = default);
    Task DeleteBasketAsync(CancellationToken ct = default);
    Task DeleteBasketAsync(Guid buyerId, CancellationToken ct = default);
}
