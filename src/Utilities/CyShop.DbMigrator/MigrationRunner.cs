using Auth.Infrastructure.Services;
using Catalog.Infrastructure.Data;
using Customers.Infrastructure.Data;
using Cyshop.Common.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Orders.Infrastructure.Data;
using StackExchange.Redis;

namespace CyShop.DbMigrator;

public class MigrationRunner(
    CatalogDbContext catalogContext,
    CustomersDbContext customersContext,
    OrdersDbContext ordersContext,
    AuthSeeder authSeeder,
    CatalogApiSeeder catalogApiSeeder,
    CustomersDataSeeder customersSeeder,
    StorageSeeder storageSeeder,
    OpenSearchSeeder openSearchSeeder,
    IIdentityProviderService identityProviderService,
    IStorageService storageService,
    IConnectionMultiplexer redis,
    CommandLineOptions options,
    IConfiguration configuration,
    ILogger<MigrationRunner> logger)
{
    public async Task RunAllAsync(CancellationToken ct = default)
    {
        logger.LogInformation("Starting database migrations and seeding...");

        if (options.Override)
        {
            logger.LogWarning("Override mode enabled — wiping all data before re-seeding.");
            await CleanAllDataAsync(ct);
        }

        await MigrateCatalogAsync(ct);
        await MigrateCustomersAsync(ct);
        await MigrateOrdersAsync(ct);
        await SeedCustomersAsync(ct);
        await authSeeder.SeedAsync(ct);
        await catalogApiSeeder.SeedAsync(ct);
        await SeedStorageAsync();
        await SeedOpenSearchAsync(ct);

        logger.LogInformation("All migrations and seeding completed.");
    }

    private async Task CleanAllDataAsync(CancellationToken ct)
    {
        // 1. Delete Keycloak realm
        var realmName = configuration["Keycloak:Realm"] ?? "cyshop";
        logger.LogInformation("[Override] Deleting Keycloak realm '{Realm}'...", realmName);
        await identityProviderService.DeleteRealmAsync(realmName);

        // 2. Wipe Catalog DB tables
        logger.LogInformation("[Override] Wiping Catalog database...");
        await catalogContext.Database.EnsureDeletedAsync(ct);

        // 3. Wipe Customers DB tables
        logger.LogInformation("[Override] Wiping Customers database...");
        await customersContext.Database.EnsureDeletedAsync(ct);

        // 4. Wipe Orders DB tables
        logger.LogInformation("[Override] Wiping Orders database...");
        await ordersContext.Database.EnsureDeletedAsync(ct);

        // 5. Flush Redis basket database (db 0 — default, matching Basket.API's redis.GetDatabase())
        logger.LogInformation("[Override] Flushing Redis basket database...");
        var endpoints = redis.GetEndPoints();
        foreach (var endpoint in endpoints)
        {
            var server = redis.GetServer(endpoint);
            await server.FlushDatabaseAsync(0);
        }

        // 6. Delete all objects in storage bucket
        var bucketName = configuration["Storage:BucketName"] ?? "catalog-images";
        logger.LogInformation("[Override] Deleting all objects in bucket '{Bucket}'...", bucketName);
        await storageService.DeleteAllObjectsAsync(bucketName, ct);

        logger.LogInformation("[Override] All data wiped.");
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

    private async Task MigrateOrdersAsync(CancellationToken ct)
    {
        logger.LogInformation("[Orders] Applying migrations...");
        await ordersContext.Database.MigrateAsync(ct);
        logger.LogInformation("[Orders] Migrations applied.");
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

    private async Task SeedOpenSearchAsync(CancellationToken ct)
    {
        logger.LogInformation("[OpenSearch] Seeding...");
        await openSearchSeeder.SeedAsync(ct);
        logger.LogInformation("[OpenSearch] Seeding complete.");
    }
}
