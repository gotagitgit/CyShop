using System.Text.Json;
using OpenSearch.Client;
using OpenSearch.Net;
using SearchServices.Models;
using SearchServices.Serialization;

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

    public async Task<IReadOnlyList<CatalogIndexDocument>> SearchAsync(
        string indexName,
        string query,
        string searchPipeline,
        string modelId,
        int maxResults,
        CancellationToken cancellationToken)
    {
        var body = new
        {
            size = maxResults,
            query = new
            {
                hybrid = new
                {
                    queries = new object[]
                    {
                        new { match = new { nameDescription = query } },
                        new { neural = new { embedding = new { query_text = query, model_id = modelId, k = maxResults } } }
                    }
                }
            }
        };

        var response = await client.LowLevel.DoRequestAsync<StringResponse>(
            OpenSearch.Net.HttpMethod.POST,
            $"/{indexName}/_search",
            cancellationToken,
            SerializeBody(body),
            new SearchRequestParameters { QueryString = { { "search_pipeline", searchPipeline } } });

        if (!response.Success)
            throw new InvalidOperationException($"OpenSearch search failed: {response.DebugInformation}");

        var jsonDoc = JsonDocument.Parse(response.Body);
        var hits = jsonDoc.RootElement.GetProperty("hits").GetProperty("hits");

        var documents = new List<CatalogIndexDocument>();
        foreach (var hit in hits.EnumerateArray())
        {
            var source = hit.GetProperty("_source");
            var doc = JsonSerializer.Deserialize<CatalogIndexDocument>(source.GetRawText());
            if (doc is not null)
                documents.Add(doc);
        }

        return documents;
    }

    private static PostData SerializeBody<T>(T data)
    {
        using var ms = new MemoryStream();
        SystemTextJsonSerializer.Instance.Serialize(data, ms);
        return PostData.Bytes(ms.ToArray());
    }
}
