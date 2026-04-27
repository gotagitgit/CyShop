using Microsoft.Extensions.Options;
using SearchServices.Factory;
using SearchServices.Models;
using SearchServices.Services.SearchClients;
using SearchServices.Settings;

namespace SearchServices.Services;

internal sealed class OpenSearchIndexService(
    IOpenSearchIndexClientWrapper wrapper,
    IIndexMappingFactory<CatalogIndexDocument> catalogMappingFactory,
    IOptions<SearchSettings> options) : IOpenSearchIndexService
{
    public async Task<bool> IndexExistsAsync(string indexName, CancellationToken cancellationToken = default)
    {
        return await wrapper.IndexExistsAsync(indexName, cancellationToken);
    }

    public async Task<OpenSearchResponse> DeleteIndexAsync(string indexName, CancellationToken cancellationToken = default)
    {
        return await wrapper.DeleteIndexAsync(indexName, cancellationToken);
    }

    public async Task<OpenSearchResponse> CreateCatalogIndexAsync(
        string indexName,
        CancellationToken cancellationToken = default)
    {
        var settings = options.Value;

        return await wrapper.CreateIndexAsync<CatalogIndexDocument>(
            indexName,
            settings.IngestPipeline,
            descriptor => catalogMappingFactory.Create(descriptor),
            cancellationToken);
    }

    public async Task<OpenSearchResponse> IndexDocumentAsync(
        string indexName,
        CatalogIndexDocument document,
        CancellationToken cancellationToken = default)
    {
        return await wrapper.BulkIndexAsync(indexName, [document], d => d.Id, cancellationToken);
    }

    public async Task<OpenSearchResponse> BulkIndexDocumentsAsync(
        string indexName,
        IReadOnlyList<CatalogIndexDocument> documents,
        CancellationToken cancellationToken = default)
    {
        return await wrapper.BulkIndexAsync(indexName, documents, d => d.Id, cancellationToken);
    }

    public async Task<IReadOnlyList<CatalogIndexDocument>> SearchAsync(
        string query,
        int maxResults = 5,
        CancellationToken cancellationToken = default)
    {
        var settings = options.Value;
        return await wrapper.SearchAsync("catalog", query, settings.SearchPipeline, maxResults, cancellationToken);
    }
}
