namespace Chat.Infrastructure.Services;

using Chat.Domain.Entities;
using Chat.Domain.Interfaces;
using SearchServices.Services;

public class OpenSearchCatalogService(
    IOpenSearchIndexService indexService) : ISearchCatalogService
{
    public async Task<IReadOnlyList<ChatProduct>> SearchAsync(string query, CancellationToken ct = default)
    {
        var documents = await indexService.SearchAsync(query, maxResults: 5, ct);
        return documents.Select(d => new ChatProduct(d.Id, d.Name, d.Price)).ToList();
    }
}
