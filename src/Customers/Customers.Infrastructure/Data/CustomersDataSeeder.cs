using Customers.Infrastructure.Data.Dtos;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Customers.Infrastructure.Data;

public record CustomerSeedEntry(Guid ExternalId, string FirstName, string LastName, string Email, string ContactNumber);

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

            var customers = CreateDevCustomers();

            await context.Customers.AddRangeAsync(customers, ct);
            await context.SaveChangesAsync(ct);

            var addressCount = customers.SelectMany(x => x.Addresses).Count();
            logger.LogInformation("Seeded {CustomerCount} customers with {AddressCount} addresses",
                customers.Length, addressCount);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error seeding customers data");
        }
    }

    private static CustomerDto[] CreateDevCustomers()
    {
        Guid[] customerIds = [Guid.NewGuid(), Guid.NewGuid()];

        return [
            new CustomerDto()
            {
                Id = customerIds[0],
                ExternalId = Guid.NewGuid(),
                FirstName = "user",
                LastName = "user",
                Email = "user@email.com",
                ContactNumber = "555-0001",
                Addresses = [.. CreateDevCustomerAddress1(customerIds[0])]
            },
            new CustomerDto()
            {
                Id = customerIds[1],
                ExternalId = Guid.NewGuid(),
                FirstName = "admin",
                LastName = "admin",
                Email = "admin@email.com",
                ContactNumber = "555-0002",
                Addresses = [.. CreateDevCustomerAddress2(customerIds[1])]
            }
        ];
    }

    private static CustomerAddressDto[] CreateDevCustomerAddress1(Guid customerId)
    {
        return [
            new CustomerAddressDto()
            {
                Id = Guid.NewGuid(),
                CustomerId = customerId,
                Label = "Home",
                Street = "789 Admin Ave",
                City = "Chicago",
                State = "IL",
                Country = "US",
                ZipCode = "60601",
                IsDefault = true,
                CreatedDateTime = DateTime.UtcNow
            },
            new CustomerAddressDto()
            {
                Id = Guid.NewGuid(),
                CustomerId = customerId,
                Label = "Office",
                Street = "321 Corporate Dr",
                City = "Chicago",
                State = "IL",
                Country = "US",
                ZipCode = "60602",
                IsDefault = false,
                CreatedDateTime = DateTime.UtcNow
            }
        ];
    }

    private static CustomerAddressDto[] CreateDevCustomerAddress2(Guid customerId)
    {
        return [
            new CustomerAddressDto()
            {
                Id = Guid.NewGuid(),
                CustomerId = customerId,
                Label = "Home",
                Street = "123 Main St",
                City = "SpringField",
                State = "IL",
                Country = "US",
                ZipCode = "62701",
                IsDefault = true,
                CreatedDateTime = DateTime.UtcNow
            },
            new CustomerAddressDto()
            {
                Id = Guid.NewGuid(),
                CustomerId = customerId,
                Label = "Work",
                Street = "456 Office Blvd",
                City = "Springfield",
                State = "IL",
                Country = "US",
                ZipCode = "62702",
                IsDefault = false,
                CreatedDateTime = DateTime.UtcNow
            }
        ];
    }
}
