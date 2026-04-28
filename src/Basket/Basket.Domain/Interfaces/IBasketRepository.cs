using Basket.Domain.Entities;

namespace Basket.Domain.Interfaces;

public interface IBasketRepository
{
    Task<CustomerBasket?> GetBasketAsync(Guid buyerId);
    Task<CustomerBasket?> UpdateBasketAsync(CustomerBasket basket);
    Task<bool> DeleteBasketAsync(Guid buyerId);
}
