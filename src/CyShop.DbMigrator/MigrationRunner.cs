using Catalog.Infrastructure.Data;
using Customers.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CyShop.DbMigrator;

public class MigrationRunner(
    CatalogDbContext catalogContext,
    DataSeeder catalogSeeder,
    CustomersDbContext customersContext,
    CustomersDataSeeder customersSeeder,
    KeycloakSeeder keycloakSeeder,
    IConfiguration configuration,
    ILogger<MigrationRunner> logger)
{
    public async Task RunAllAsync()
    {
        logger.LogInformation("Starting database migrations and seeding...");

        await MigrateCatalogAsync();

        // Seed Keycloak first to get user IDs (sub claims)
        var keycloakUsers = await SeedKeycloakAsync();

        // Then seed Customers with the Keycloak external IDs
        await MigrateCustomersAsync(keycloakUsers);

        logger.LogInformation("All migrations and seeding completed.");
    }

    private async Task<IReadOnlyList<KeycloakUser>> SeedKeycloakAsync()
    {
        try
        {
            return await keycloakSeeder.SeedAsync();
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "[Keycloak] Seeding failed — Keycloak may not be running. Skipping.");
            return [];
        }
    }

    private async Task MigrateCatalogAsync()
    {
        logger.LogInformation("[Catalog] Applying migrations...");
        await catalogContext.Database.MigrateAsync();
        logger.LogInformation("[Catalog] Migrations applied.");

        logger.LogInformation("[Catalog] Seeding data...");
        var seedPath = configuration["SeedData:CatalogJsonPath"]
            ?? Path.Combine(AppContext.BaseDirectory, "SeedData", "catalog.json");
        await catalogSeeder.SeedAsync(seedPath);
        logger.LogInformation("[Catalog] Seeding complete.");
    }

    private async Task MigrateCustomersAsync(IReadOnlyList<KeycloakUser> keycloakUsers)
    {
        logger.LogInformation("[Customers] Applying migrations...");
        await customersContext.Database.MigrateAsync();
        logger.LogInformation("[Customers] Migrations applied.");

        // Map Keycloak users to seed entries with their external IDs
        IReadOnlyList<CustomerSeedEntry>? entries = keycloakUsers.Count > 0
            ? keycloakUsers.Select(u => u.Username switch
            {
                "user" => new CustomerSeedEntry(u.KeycloakId, "Test", "User", u.Email, "555-0001"),
                "admin" => new CustomerSeedEntry(u.KeycloakId, "Test", "Admin", u.Email, "555-0002"),
                _ => new CustomerSeedEntry(u.KeycloakId, u.Username, u.Username, u.Email, "555-0000")
            }).ToList()
            : null; // null = use placeholder fallback in seeder

        logger.LogInformation("[Customers] Seeding data...");
        await customersSeeder.SeedAsync(entries);
        logger.LogInformation("[Customers] Seeding complete.");
    }
}
