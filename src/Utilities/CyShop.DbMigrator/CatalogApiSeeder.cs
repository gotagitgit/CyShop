using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CyShop.DbMigrator;

public class CatalogApiSeeder(
    HttpClient httpClient,
    IConfiguration configuration,
    ILogger<CatalogApiSeeder> logger)
{
    public async Task SeedAsync(CancellationToken ct = default)
    {
        var baseUrl = configuration["ApiEndpoints:CatalogApi"]
            ?? throw new InvalidOperationException("ApiEndpoints:CatalogApi is not configured");

        httpClient.BaseAddress = new Uri(baseUrl);

        var jsonPath = Path.Combine(AppContext.BaseDirectory, "SeedData", "catalog_data", "catalog.json");

        if (!File.Exists(jsonPath))
        {
            logger.LogError("Catalog seed file not found at {Path}", jsonPath);
            return;
        }

        await using var stream = File.OpenRead(jsonPath);
        var seedItems = await JsonSerializer.DeserializeAsync<List<CatalogSeedItem>>(stream, cancellationToken: ct);

        if (seedItems is null || seedItems.Count == 0)
        {
            logger.LogWarning("No catalog items found in seed file at {Path}", jsonPath);
            return;
        }

        // Check if catalog already has data
        var existingResponse = await httpClient.GetAsync("/api/catalog", ct);
        if (existingResponse.IsSuccessStatusCode)
        {
            var existing = await existingResponse.Content.ReadFromJsonAsync<List<JsonElement>>(cancellationToken: ct);
            if (existing is { Count: > 0 })
            {
                logger.LogInformation("Catalog already contains {Count} items. Skipping seed.", existing.Count);
                return;
            }
        }

        logger.LogInformation("Seeding {Count} catalog items via API...", seedItems.Count);

        foreach (var item in seedItems)
        {
            var dto = new
            {
                item.Name,
                item.Description,
                item.Price,
                BrandName = item.Brand,
                TypeName = item.Type,
                ImagePath = $"images/{item.Id}.webp"
            };

            var response = await httpClient.PostAsJsonAsync("/api/catalog", dto, ct);

            if (response.IsSuccessStatusCode)
            {
                logger.LogInformation("Seeded catalog item: {Name}", item.Name);
            }
            else
            {
                var body = await response.Content.ReadAsStringAsync(ct);
                logger.LogError(
                    "Failed to seed catalog item '{Name}': {StatusCode} - {Body}",
                    item.Name, response.StatusCode, body);
            }
        }

        logger.LogInformation("Catalog API seeding complete.");
    }

    private sealed class CatalogSeedItem
    {
        public int Id { get; set; }
        public string Type { get; set; } = string.Empty;
        public string Brand { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
    }
}
