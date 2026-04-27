using System.ComponentModel;
using System.Text.Json;
using Chat.Domain.Entities;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using SearchServices.Services;

namespace Chat.Infrastructure.Tools;

public class SearchCatalogTool(
    IOpenSearchIndexService indexService,
    ILogger<SearchCatalogTool> logger) : IChatTool
{
    private IReadOnlyList<ChatProduct> _lastResults = [];

    public AIFunction Create() =>
        AIFunctionFactory.Create(SearchAsync, "SearchCatalog",
            "Search the CyShop product catalog by natural language query. Returns product names and prices.");

    [Description("Search the product catalog")]
    private async Task<string> SearchAsync(
        [Description("The search query")] string query,
        CancellationToken ct = default)
    {
        logger.LogInformation("Tool called: SearchCatalog('{Query}')", query);

        var documents = await indexService.SearchAsync(query, maxResults: 5, ct);
        //_lastResults = documents.Select(d => new ChatProduct(d.Id, d.Name, d.Price)).ToList();

        if (documents.Count == 0)
            return "No documents found matching the query.";

        return JsonSerializer.Serialize(documents);
    }
}
