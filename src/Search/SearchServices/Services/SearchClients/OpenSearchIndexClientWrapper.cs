using OpenSearch.Client;
using SearchServices.Models;

namespace SearchServices.Services.SearchClients;

internal sealed class OpenSearchIndexClientWrapper(IOpenSearchClient client) : IOpenSearchIndexClientWrapper
{
    public async Task<bool> IndexExistsAsync(string indexName, CancellationToken cancellationToken)
    {
        var response = await client.Indices.ExistsAsync(indexName, ct: cancellationToken);
        return response.Exists;
    }

    public async Task<OpenSearchResponse> DeleteIndexAsync(string indexName, CancellationToken cancellationToken)
    {
        var response = await client.Indices.DeleteAsync(indexName, ct: cancellationToken);
        return new OpenSearchResponse(
            response.IsValid,
            response.ApiCall?.HttpStatusCode ?? 0,
            response.DebugInformation);
    }

    public async Task<OpenSearchResponse> CreateIndexAsync<T>(
        string indexName,
        string pipelineName,
        Func<TypeMappingDescriptor<T>, ITypeMapping> mappingSelector,
        CancellationToken cancellationToken) where T : class
    {
        var response = await client.Indices.CreateAsync(indexName, c => c
            .Settings(s => s
                .Setting("index.knn", true)
                .Setting("index.default_pipeline", pipelineName)
            )
            .Map(mappingSelector),
            cancellationToken);

        return new OpenSearchResponse(
            response.IsValid,
            response.ApiCall?.HttpStatusCode ?? 0,
            response.DebugInformation);
    }

    public async Task<OpenSearchResponse> BulkIndexAsync<T>(
        string indexName,
        IReadOnlyList<T> documents,
        Func<T, string> idSelector,
        CancellationToken cancellationToken) where T : class
    {
        var response = await client.BulkAsync(b =>
        {
            foreach (var doc in documents)
            {
                b.Index<T>(i => i
                    .Index(indexName)
                    .Id(idSelector(doc))
                    .Document(doc));
            }
            return b;
        }, cancellationToken);

        return new OpenSearchResponse(
            response.IsValid,
            response.ApiCall?.HttpStatusCode ?? 0,
            response.DebugInformation);
    }
}
