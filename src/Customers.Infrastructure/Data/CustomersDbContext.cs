using Customers.Infrastructure.Data.Configurations;
using Customers.Infrastructure.Data.Dtos;
using Microsoft.EntityFrameworkCore;

namespace Customers.Infrastructure.Data;

public class CustomersDbContext(DbContextOptions<CustomersDbContext> options) : DbContext(options)
{
    public DbSet<CustomerDto> Customers => Set<CustomerDto>();
    public DbSet<CustomerAddressDto> CustomerAddresses => Set<CustomerAddressDto>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new CustomerConfiguration());
        modelBuilder.ApplyConfiguration(new CustomerAddressConfiguration());
    }
}
