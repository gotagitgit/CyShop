using Customers.Infrastructure.Data.Dtos;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Customers.Infrastructure.Data;

public record CustomerSeedEntry(Guid ExternalId, string FirstName, string LastName, string Email, string ContactNumber);

public class CustomersDataSeeder(CustomersDbContext context, ILogger<CustomersDataSeeder> logger)
{
    /// <summary>
    /// Seeds customers with their Keycloak external IDs and addresses.
    /// </summary>
    public async Task SeedAsync(IReadOnlyList<CustomerSeedEntry>? entries = null, CancellationToken ct = default)
    {
        try
        {
            if (await context.Customers.AnyAsync(ct))
            {
                logger.LogInformation("Customers database already contains data. Skipping seed.");
                return;
            }

            // Fall back to deterministic GUIDs if Keycloak wasn't available
            entries ??= [
                new(Guid.NewGuid(), "Test", "User", "user@email.com", "555-0001"),
                new(Guid.NewGuid(), "Test", "Admin", "admin@email.com", "555-0002")
            ];

            var customerIds = new Dictionary<string, Guid>();

            var customers = entries.Select(e =>
            {
                var id = Guid.NewGuid();
                customerIds[e.Email] = id;
                return new CustomerDto
                {
                    Id = id,
                    ExternalId = e.ExternalId,
                    FirstName = e.FirstName,
                    LastName = e.LastName,
                    Email = e.Email,
                    ContactNumber = e.ContactNumber
                };
            }).ToList();

            await context.Customers.AddRangeAsync(customers, ct);
            await context.SaveChangesAsync(ct);

            var addresses = new List<CustomerAddressDto>();

            if (customerIds.TryGetValue("user@email.com", out var userId))
            {
                addresses.Add(new CustomerAddressDto
                {
                    Id = Guid.NewGuid(), CustomerId = userId, Label = "Home",
                    Street = "123 Main St", City = "Springfield", State = "IL",
                    Country = "US", ZipCode = "62701", IsDefault = true, CreatedDateTime = DateTime.UtcNow
                });
                addresses.Add(new CustomerAddressDto
                {
                    Id = Guid.NewGuid(), CustomerId = userId, Label = "Work",
                    Street = "456 Office Blvd", City = "Springfield", State = "IL",
                    Country = "US", ZipCode = "62702", IsDefault = false, CreatedDateTime = DateTime.UtcNow
                });
            }

            if (customerIds.TryGetValue("admin@email.com", out var adminId))
            {
                addresses.Add(new CustomerAddressDto
                {
                    Id = Guid.NewGuid(), CustomerId = adminId, Label = "Home",
                    Street = "789 Admin Ave", City = "Chicago", State = "IL",
                    Country = "US", ZipCode = "60601", IsDefault = true, CreatedDateTime = DateTime.UtcNow
                });
                addresses.Add(new CustomerAddressDto
                {
                    Id = Guid.NewGuid(), CustomerId = adminId, Label = "Office",
                    Street = "321 Corporate Dr", City = "Chicago", State = "IL",
                    Country = "US", ZipCode = "60602", IsDefault = false, CreatedDateTime = DateTime.UtcNow
                });
            }

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
