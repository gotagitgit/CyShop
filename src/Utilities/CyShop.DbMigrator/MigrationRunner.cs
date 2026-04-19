using Auth.Infrastructure.Services;
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
    AuthSeeder authSeeder,
    IIdentityProviderService identityProviderService,
    IConfiguration configuration,
    ILogger<MigrationRunner> logger)
{
    public async Task RunAllAsync()
    {
        logger.LogInformation("Starting database migrations and seeding...");

        await MigrateCatalogAsync();

        await MigrateCustomersAsync();

        await authSeeder.SeedAsync(CancellationToken.None);

        logger.LogInformation("All migrations and seeding completed.");
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

    private async Task MigrateCustomersAsync()
    {
        logger.LogInformation("[Customers] Applying migrations...");
        await customersContext.Database.MigrateAsync();
        logger.LogInformation("[Customers] Migrations applied.");

        logger.LogInformation("[Customers] Seeding data...");
        await customersSeeder.SeedAsync();
        logger.LogInformation("[Customers] Seeding complete.");
    }
}
