using Basket.Domain.Interfaces;
using Basket.Infrastructure.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace Basket.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
    {
        services.AddScoped<IBasketRepository, RedisBasketRepository>();
        return services;
    }
}
