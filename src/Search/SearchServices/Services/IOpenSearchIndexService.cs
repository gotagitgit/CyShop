using SearchServices.Models;

namespace SearchServices.Services;

public interface IOpenSearchIndexService
{
    Task<bool> IndexExistsAsync(string indexName, CancellationToken cancellationToken = default);
    Task<OpenSearchResponse> DeleteIndexAsync(string indexName, CancellationToken cancellationToken = default);
    Task<OpenSearchResponse> CreateCatalogIndexAsync(string indexName, CancellationToken cancellationToken = default);
    Task<OpenSearchResponse> IndexDocumentAsync(string indexName, CatalogIndexDocument document, CancellationToken cancellationToken = default);
    Task<OpenSearchResponse> BulkIndexDocumentsAsync(string indexName, IReadOnlyList<CatalogIndexDocument> documents, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CatalogIndexDocument>> SearchAsync(
        string query,
        int maxResults = 5,
        CancellationToken cancellationToken = default);
}
