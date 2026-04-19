using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CyShop.DbMigrator;

public class CustomerApiSeeder(
    HttpClient httpClient,
    IConfiguration configuration,
    ILogger<CustomerApiSeeder> logger)
{
    public async Task SeedAsync(Dictionary<string, Guid> userKeycloakIds, CancellationToken ct = default)
    {
        var baseUrl = configuration["ApiEndpoints:CustomersApi"]
            ?? throw new InvalidOperationException("ApiEndpoints:CustomersApi is not configured");

        httpClient.BaseAddress = new Uri(baseUrl);

        var customers = BuildCustomerData();

        logger.LogInformation("Seeding {Count} customers via API...", customers.Length);

        foreach (var customer in customers)
        {
            if (!userKeycloakIds.TryGetValue(customer.Username, out var keycloakUserId))
            {
                logger.LogWarning("No Keycloak user ID found for '{Username}', skipping", customer.Username);
                continue;
            }

            await SeedCustomerAsync(customer, keycloakUserId, ct);
        }

        logger.LogInformation("Customer API seeding complete.");
    }

    private async Task SeedCustomerAsync(CustomerSeedData customer, Guid keycloakUserId, CancellationToken ct)
    {
        var profileDto = new
        {
            customer.FirstName,
            customer.LastName,
            customer.Email,
            customer.ContactNumber
        };

        using var profileRequest = new HttpRequestMessage(HttpMethod.Post, "/api/customers/profile");
        profileRequest.Content = JsonContent.Create(profileDto);
        profileRequest.Headers.Add("X-On-Behalf-Of", keycloakUserId.ToString());

        var profileResponse = await httpClient.SendAsync(profileRequest, ct);

        if (!profileResponse.IsSuccessStatusCode)
        {
            var body = await profileResponse.Content.ReadAsStringAsync(ct);
            logger.LogError(
                "Failed to create customer profile for '{Username}': {StatusCode} - {Body}",
                customer.Username, profileResponse.StatusCode, body);
            return;
        }

        logger.LogInformation("Created customer profile for '{Username}'", customer.Username);

        foreach (var address in customer.Addresses)
        {
            using var addressRequest = new HttpRequestMessage(HttpMethod.Post, "/api/customers/addresses");
            addressRequest.Content = JsonContent.Create(address);
            addressRequest.Headers.Add("X-On-Behalf-Of", keycloakUserId.ToString());

            var addressResponse = await httpClient.SendAsync(addressRequest, ct);

            if (addressResponse.IsSuccessStatusCode)
            {
                logger.LogInformation(
                    "Created address '{Label}' for '{Username}'",
                    address.Label, customer.Username);
            }
            else
            {
                var body = await addressResponse.Content.ReadAsStringAsync(ct);
                logger.LogError(
                    "Failed to create address '{Label}' for '{Username}': {StatusCode} - {Body}",
                    address.Label, customer.Username, addressResponse.StatusCode, body);
            }
        }
    }

    private static CustomerSeedData[] BuildCustomerData() =>
    [
        new(
            Username: "user",
            FirstName: "user",
            LastName: "user",
            Email: "user@email.com",
            ContactNumber: "555-0001",
            Addresses:
            [
                new("Home", "789 Admin Ave", "Chicago", "IL", "US", "60601", true),
                new("Office", "321 Corporate Dr", "Chicago", "IL", "US", "60602", false)
            ]),
        new(
            Username: "admin",
            FirstName: "admin",
            LastName: "admin",
            Email: "admin@email.com",
            ContactNumber: "555-0002",
            Addresses:
            [
                new("Home", "123 Main St", "SpringField", "IL", "US", "62701", true),
                new("Work", "456 Office Blvd", "Springfield", "IL", "US", "62702", false)
            ])
    ];

    private sealed record CustomerSeedData(
        string Username,
        string FirstName,
        string LastName,
        string Email,
        string ContactNumber,
        AddressSeedData[] Addresses);

    private sealed record AddressSeedData(
        string Label,
        string Street,
        string City,
        string State,
        string Country,
        string ZipCode,
        bool IsDefault);
}
