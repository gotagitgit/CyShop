using System.Text.Json;
using Basket.API.Models;
using StackExchange.Redis;

namespace Basket.API.Repositories;

public sealed class RedisBasketRepository(IConnectionMultiplexer redis) : IBasketRepository
{
    private const string KeyPrefix = "basket:";
    private readonly IDatabase _database = redis.GetDatabase();

    private static string Key(string buyerId) => $"{KeyPrefix}{buyerId}";

    public async Task<CustomerBasket?> GetBasketAsync(string buyerId)
    {
        var data = await _database.StringGetAsync(Key(buyerId));

        if (data.IsNullOrEmpty)
        {
            return null;
        }

        return JsonSerializer.Deserialize<CustomerBasket>(data.ToString());
    }

    public async Task<CustomerBasket?> UpdateBasketAsync(CustomerBasket basket)
    {
        var json = JsonSerializer.Serialize(basket);
        var created = await _database.StringSetAsync(Key(basket.BuyerId), json);

        if (!created)
        {
            return null;
        }

        return await GetBasketAsync(basket.BuyerId);
    }

    public async Task<bool> DeleteBasketAsync(string buyerId)
    {
        return await _database.KeyDeleteAsync(Key(buyerId));
    }
}
