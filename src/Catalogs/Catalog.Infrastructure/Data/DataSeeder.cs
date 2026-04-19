using System.Text.Json;
using Catalog.Infrastructure.Data.Dtos;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Catalog.Infrastructure.Data;

public class DataSeeder(CatalogDbContext context, ILogger<DataSeeder> logger)
{
    public async Task SeedAsync(CancellationToken ct = default)
    {
        var jsonPath = Path.Combine(AppContext.BaseDirectory, "SeedData", "catalog_data", "catalog.json");
        try
        {
            if (await context.CatalogItems.AnyAsync(ct))
            {
                logger.LogInformation("Database already contains catalog items. Skipping seed.");
                return;
            }

            if (!File.Exists(jsonPath))
            {
                logger.LogError("Catalog seed file not found at {Path}", jsonPath);
                return;
            }

            await using var stream = File.OpenRead(jsonPath);
            var seedItems = await JsonSerializer.DeserializeAsync<List<CatalogSeedDto>>(stream, cancellationToken: ct);

            if (seedItems is null || seedItems.Count == 0)
            {
                logger.LogError("No catalog items found in seed file at {Path}", jsonPath);
                return;
            }

            // Build brand and type lookup dictionaries with new Guids
            var brandMap = seedItems
                .Select(i => i.Brand)
                .Distinct()
                .ToDictionary(name => name, name => new CatalogBrandDto { Id = Guid.NewGuid(), Name = name });

            var typeMap = seedItems
                .Select(i => i.Type)
                .Distinct()
                .ToDictionary(name => name, name => new CatalogTypeDto { Id = Guid.NewGuid(), Name = name });

            await context.CatalogBrands.AddRangeAsync(brandMap.Values, ct);
            await context.CatalogTypes.AddRangeAsync(typeMap.Values, ct);

            var itemDtos = seedItems.Select(seed => new CatalogItemDto
            {
                Id = Guid.NewGuid(),
                Name = seed.Name,
                Description = seed.Description,
                Price = seed.Price,
                ImagePath = $"images/{seed.Id}.webp",
                BrandId = brandMap[seed.Brand].Id,
                TypeId = typeMap[seed.Type].Id,
            }).ToArray();

            await context.CatalogItems.AddRangeAsync(itemDtos, ct);
            await context.SaveChangesAsync(ct);

            logger.LogInformation("Seeded {Count} catalog items", itemDtos.Length);
        }
        catch (JsonException ex)
        {
            logger.LogError(ex, "Invalid JSON in catalog seed file at {Path}", jsonPath);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error seeding catalog data from {Path}", jsonPath);
        }
    }
}
