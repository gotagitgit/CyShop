using Customers.Infrastructure.Data.Dtos;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Customers.Infrastructure.Data;

public class CustomersDataSeeder(CustomersDbContext context, ILogger<CustomersDataSeeder> logger)
{
    public async Task SeedAsync(CancellationToken ct = default)
    {
        try
        {
            if (await context.Customers.AnyAsync(ct))
            {
                logger.LogInformation("Customers database already contains data. Skipping seed.");
                return;
            }

            var userCustomerId = Guid.NewGuid();
            var adminCustomerId = Guid.NewGuid();

            var customers = new List<CustomerDto>
            {
                new()
                {
                    Id = userCustomerId,
                    FirstName = "Test",
                    LastName = "User",
                    Email = "user@email.com",
                    ContactNumber = "555-0001"
                },
                new()
                {
                    Id = adminCustomerId,
                    FirstName = "Test",
                    LastName = "Admin",
                    Email = "admin@email.com",
                    ContactNumber = "555-0002"
                }
            };

            await context.Customers.AddRangeAsync(customers, ct);
            await context.SaveChangesAsync(ct);

            var addresses = new List<CustomerAddressDto>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    CustomerId = userCustomerId,
                    Label = "Home",
                    Street = "123 Main St",
                    City = "Springfield",
                    State = "IL",
                    Country = "US",
                    ZipCode = "62701",
                    IsDefault = true,
                    CreatedDateTime = DateTime.UtcNow
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    CustomerId = userCustomerId,
                    Label = "Work",
                    Street = "456 Office Blvd",
                    City = "Springfield",
                    State = "IL",
                    Country = "US",
                    ZipCode = "62702",
                    IsDefault = false,
                    CreatedDateTime = DateTime.UtcNow
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    CustomerId = adminCustomerId,
                    Label = "Home",
                    Street = "789 Admin Ave",
                    City = "Chicago",
                    State = "IL",
                    Country = "US",
                    ZipCode = "60601",
                    IsDefault = true,
                    CreatedDateTime = DateTime.UtcNow
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    CustomerId = adminCustomerId,
                    Label = "Office",
                    Street = "321 Corporate Dr",
                    City = "Chicago",
                    State = "IL",
                    Country = "US",
                    ZipCode = "60602",
                    IsDefault = false,
                    CreatedDateTime = DateTime.UtcNow
                }
            };

            await context.CustomerAddresses.AddRangeAsync(addresses, ct);
            await context.SaveChangesAsync(ct);

            logger.LogInformation("Seeded {CustomerCount} customers with {AddressCount} addresses",
                customers.Count, addresses.Count);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error seeding customers data");
        }
    }
}
