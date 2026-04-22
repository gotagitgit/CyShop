using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Orders.Domain.Interfaces;
using Orders.Infrastructure.Data;
using Orders.Infrastructure.Repositories;

namespace Orders.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<OrdersDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("OrdersDb")));

        services.AddScoped<IOrderRepository, OrderRepository>();

        return services;
    }
}
