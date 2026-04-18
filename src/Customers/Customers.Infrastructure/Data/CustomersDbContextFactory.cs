using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Customers.Infrastructure.Data;

public class CustomersDbContextFactory : IDesignTimeDbContextFactory<CustomersDbContext>
{
    public CustomersDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<CustomersDbContext>()
            .UseNpgsql("Host=localhost;Port=5432;Database=CyshopCustomers;Username=postgres;Password=postgres")
            .Options;

        return new CustomersDbContext(options);
    }
}
