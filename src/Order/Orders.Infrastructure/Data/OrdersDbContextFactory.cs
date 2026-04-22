using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Orders.Infrastructure.Data;

public class OrdersDbContextFactory : IDesignTimeDbContextFactory<OrdersDbContext>
{
    public OrdersDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<OrdersDbContext>()
            .UseNpgsql("Host=localhost;Database=CyshopOrders")
            .Options;

        return new OrdersDbContext(options);
    }
}
