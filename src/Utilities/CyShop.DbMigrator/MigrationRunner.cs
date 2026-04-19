using Catalog.Infrastructure.Data;
using Customers.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CyShop.DbMigrator;

public class MigrationRunner(
    CatalogDbContext catalogContext,
    CustomersDbContext customersContext,
    AuthSeeder authSeeder,
    CatalogApiSeeder catalogApiSeeder,
    CustomersDataSeeder customersSeeder,
    StorageSeeder storageSeeder,
    ILogger<MigrationRunner> logger)
{
    public async Task RunAllAsync(CancellationToken ct = default)
    {
        logger.LogInformation("Starting database migrations and seeding...");

        await MigrateCatalogAsync(ct);

        await MigrateCustomersAsync(ct);

        await SeedCustomersAsync(ct);

        await authSeeder.SeedAsync(ct);

        await catalogApiSeeder.SeedAsync(ct);

        await SeedStorageAsync();

        logger.LogInformation("All migrations and seeding completed.");
    }

    private async Task MigrateCatalogAsync(CancellationToken ct)
    {
        logger.LogInformation("[Catalog] Applying migrations...");
        await catalogContext.Database.MigrateAsync(ct);
        logger.LogInformation("[Catalog] Migrations applied.");
    }

    private async Task MigrateCustomersAsync(CancellationToken ct)
    {
        logger.LogInformation("[Customers] Applying migrations...");
        await customersContext.Database.MigrateAsync(ct);
        logger.LogInformation("[Customers] Migrations applied.");
    }

    private async Task SeedCustomersAsync(CancellationToken ct)
    {
        logger.LogInformation("[Customers] Seeding data...");
        await customersSeeder.SeedAsync();
        logger.LogInformation("[Customers] Seeding complete.");
    }

    private async Task SeedStorageAsync()
    {
        logger.LogInformation("[Storage] Seeding images...");
        await storageSeeder.SeedAsync();
        logger.LogInformation("[Storage] Image seeding complete.");
    }
}
