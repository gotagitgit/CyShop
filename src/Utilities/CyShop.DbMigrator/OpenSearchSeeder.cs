using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SearchServices.Models;
using SearchServices.Services;

namespace CyShop.DbMigrator;

public class OpenSearchSeeder(
    OpenSearchModelSetup modelSetup,
    IOpenSearchIndexService indexService,
    HttpClient httpClient,
    IConfiguration configuration,
    ILogger<OpenSearchSeeder> logger)
{
    private const string IndexName = "catalog";

    public async Task SeedAsync(CancellationToken ct = default)
    {
        logger.LogInformation("[OpenSearch] Starting setup...");
        await modelSetup.RunAsync(ct);

        // Delete existing index and recreate
        if (await indexService.IndexExistsAsync(IndexName, ct))
        {
            logger.LogInformation("[OpenSearch] Deleting existing index '{Index}'...", IndexName);
            await indexService.DeleteIndexAsync(IndexName, ct);
        }

        logger.LogInformation("[OpenSearch] Creating index '{Index}'...", IndexName);
        var createResult = await indexService.CreateCatalogIndexAsync(IndexName, ct);
        if (!createResult.IsSuccess)
            throw new InvalidOperationException($"Failed to create index: {createResult.Body}");

        // Fetch catalog items from the API
        var catalogItems = await FetchCatalogItemsAsync(ct);
        if (catalogItems.Count == 0)
        {
            logger.LogWarning("[OpenSearch] No catalog items found to index.");
            return;
        }

        // Index documents
        var documents = catalogItems.Select(item => new CatalogIndexDocument
        {
            Id = item.Id,
            Name = item.Name,
            Description = item.Description,
            NameDescription = $"{item.Name} {item.Description} {item.Brand.Name} {item.Type.Name}",
            Price = item.Price,
            BrandName = item.Brand.Name,
            TypeName = item.Type.Name
        }).ToList();

        logger.LogInformation("[OpenSearch] Indexing {Count} catalog items...", documents.Count);
        var bulkResult = await indexService.BulkIndexDocumentsAsync(IndexName, documents, ct);

        if (!bulkResult.IsSuccess)
            logger.LogWarning("[OpenSearch] Bulk index had issues: {Body}", bulkResult.Body);
        else
            logger.LogInformation("[OpenSearch] Indexed {Count} catalog items.", documents.Count);

        logger.LogInformation("[OpenSearch] Setup completed.");
    }

    private async Task<IReadOnlyList<CatalogApiItem>> FetchCatalogItemsAsync(CancellationToken ct)
    {
        var baseUrl = configuration["ApiEndpoints:CatalogApi"]
            ?? throw new InvalidOperationException("ApiEndpoints:CatalogApi is not configured");

        logger.LogInformation("[OpenSearch] Fetching catalog items from {Url}...", baseUrl);

        var response = await httpClient.GetAsync($"{baseUrl}/api/catalog", ct);
        response.EnsureSuccessStatusCode();

        var items = await response.Content.ReadFromJsonAsync<List<CatalogApiItem>>(
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true }, ct);

        logger.LogInformation("[OpenSearch] Fetched {Count} catalog items.", items?.Count ?? 0);
        return items ?? [];
    }

    private sealed class CatalogApiItem
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public CatalogApiRef Type { get; set; } = new();
        public CatalogApiRef Brand { get; set; } = new();
    }

    private sealed class CatalogApiRef
    {
        public string Name { get; set; } = string.Empty;
    }
}
