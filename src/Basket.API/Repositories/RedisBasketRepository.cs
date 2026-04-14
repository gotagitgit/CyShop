using System.Text.Json;
using Basket.API.Models;
using StackExchange.Redis;

namespace Basket.API.Repositories;

public sealed class RedisBasketRepository(IConnectionMultiplexer redis) : IBasketRepository
{
    private readonly IDatabase _database = redis.GetDatabase();

    public async Task<CustomerBasket?> GetBasketAsync(string buyerId)
    {
        var data = await _database.StringGetAsync(buyerId);

        if (data.IsNullOrEmpty)
        {
            return null;
        }

        return JsonSerializer.Deserialize<CustomerBasket>(data.ToString());
    }

    public async Task<CustomerBasket?> UpdateBasketAsync(CustomerBasket basket)
    {
        var json = JsonSerializer.Serialize(basket);
        var created = await _database.StringSetAsync(basket.BuyerId, json);

        if (!created)
        {
            return null;
        }

        return await GetBasketAsync(basket.BuyerId);
    }

    public async Task<bool> DeleteBasketAsync(string buyerId)
    {
        return await _database.KeyDeleteAsync(buyerId);
    }
}
