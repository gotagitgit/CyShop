using Basket.Application.Interfaces;
using Basket.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Basket.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IBasketService, BasketService>();
        return services;
    }
}
