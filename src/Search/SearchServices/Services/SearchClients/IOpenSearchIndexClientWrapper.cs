using OpenSearch.Client;
using SearchServices.Models;

namespace SearchServices.Services.SearchClients;

public interface IOpenSearchIndexClientWrapper
{
    Task<bool> IndexExistsAsync(string indexName, CancellationToken cancellationToken);

    Task<OpenSearchResponse> DeleteIndexAsync(string indexName, CancellationToken cancellationToken);

    Task<OpenSearchResponse> CreateIndexAsync<T>(
        string indexName,
        string pipelineName,
        Func<TypeMappingDescriptor<T>, ITypeMapping> mappingSelector,
        CancellationToken cancellationToken) where T : class;

    Task<OpenSearchResponse> BulkIndexAsync<T>(
        string indexName,
        IReadOnlyList<T> documents,
        Func<T, string> idSelector,
        CancellationToken cancellationToken) where T : class;
}
