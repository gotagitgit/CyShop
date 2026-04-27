namespace Chat.Infrastructure.Plugins;

using Chat.Domain.Interfaces;
using Chat.Domain.Entities;
using Microsoft.SemanticKernel;
using System.ComponentModel;

public class SearchCatalogPlugin(ISearchCatalogService searchService)
{
    [KernelFunction, Description("Search the product catalog for a provided product description")]
    public async Task<IReadOnlyList<ChatProduct>> SearchCatalog(
        [Description("The product description for which to search")] string query,
        CancellationToken ct)
    {
        return await searchService.SearchAsync(query, ct);
    }
}
