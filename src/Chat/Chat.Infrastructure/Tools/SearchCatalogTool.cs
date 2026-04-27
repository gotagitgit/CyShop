using System.ComponentModel;
using System.Text.Json;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using SearchServices.Services;

namespace Chat.Infrastructure.Tools;

public class SearchCatalogTool(
    IOpenSearchIndexService indexService,
    ILogger<SearchCatalogTool> logger) : IChatTool
{
    public AIFunction Create() =>
        AIFunctionFactory.Create(SearchAsync, "SearchCatalog",
            "Search the CyShop product catalog by natural language query. Returns product details.");

    [Description("Searches CyShop Catalog for products matching the product description")]
    private async Task<string> SearchAsync(
        [Description("The product description for which to search")] string query,
        CancellationToken ct = default)
    {
        logger.LogInformation("Tool called: SearchCatalog('{Query}')", query);

        var documents = await indexService.SearchAsync(query, maxResults: 5, ct);

        logger.LogInformation("Search returned {Count} documents", documents.Count);

        if (documents.Count == 0)
            return "No documents found matching the query.";

        return JsonSerializer.Serialize(documents);
    }
}
