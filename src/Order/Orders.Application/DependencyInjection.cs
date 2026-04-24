using Microsoft.Extensions.DependencyInjection;
using Orders.Application.Interfaces;
using Orders.Application.Services;

namespace Orders.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<IIdempotencyService, IdempotencyService>();
        services.AddScoped<IIntegrationEventService, OrderingIntegrationEventService>();
        return services;
    }
}
