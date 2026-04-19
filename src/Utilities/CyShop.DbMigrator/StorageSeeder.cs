using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Storage.Infrastructure.Services;

namespace CyShop.DbMigrator;

public class StorageSeeder(
    IStorageService storageService,
    IConfiguration configuration,
    ILogger<StorageSeeder> logger)
{
    private readonly string _bucketName = configuration["Storage:BucketName"]
        ?? "catalog-images";

    public async Task SeedAsync(CancellationToken ct = default)
    {
        await storageService.CreateBucketIfNotExistsAsync(_bucketName, ct);

        if (await storageService.BucketHasObjectsAsync(_bucketName, ct))
        {
            logger.LogInformation("Bucket '{Bucket}' already has objects. Skipping image seed.", _bucketName);
            return;
        }

        var imagesDir = Path.Combine(AppContext.BaseDirectory, "SeedData", "images");
        foreach (var file in Directory.GetFiles(imagesDir, "*.webp"))
        {
            var key = $"images/{Path.GetFileName(file)}";
            await using var stream = File.OpenRead(file);
            await storageService.UploadAsync(_bucketName, key, stream, "image/webp", ct);
        }

        logger.LogInformation("Uploaded seed images to bucket '{Bucket}'.", _bucketName);
    }
}
