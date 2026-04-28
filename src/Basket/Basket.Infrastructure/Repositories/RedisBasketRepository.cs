using System.Text.Json;
using Basket.Domain.Entities;
using Basket.Domain.Interfaces;
using StackExchange.Redis;

namespace Basket.Infrastructure.Repositories;

public sealed class RedisBasketRepository(IConnectionMultiplexer redis) : IBasketRepository
{
    private const string KeyPrefix = "basket:";
    private readonly IDatabase _database = redis.GetDatabase();

    private static string Key(Guid buyerId) => $"{KeyPrefix}{buyerId}";

    public async Task<CustomerBasket?> GetBasketAsync(Guid buyerId)
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

    public async Task<bool> DeleteBasketAsync(Guid buyerId)
    {
        return await _database.KeyDeleteAsync(Key(buyerId));
    }
}
