using Microsoft.Extensions.Options;
using OpenSearch.Client;
using SearchServices.Models;
using SearchServices.Settings;

namespace SearchServices.Factory;

internal sealed class CatalogIndexMappingFactory(IOptions<SearchSettings> options)
    : IIndexMappingFactory<CatalogIndexDocument>
{
    public ITypeMapping Create(TypeMappingDescriptor<CatalogIndexDocument> descriptor)
    {
        var embeddingDimension = options.Value.EmbeddingDimension;

        return descriptor.Properties(p => p
            .Keyword(k => k.Name(n => n.Id))
            .Text(t => t.Name(n => n.Name))
            .Text(t => t.Name(n => n.Description))
            .Text(t => t.Name(n => n.NameDescription))
            .Number(n => n.Name(f => f.Price).Type(NumberType.Float))
            .Keyword(k => k.Name(n => n.BrandName))
            .Keyword(k => k.Name(n => n.TypeName))
            .KnnVector(knn => knn
                .Name("embedding")
                .Dimension(embeddingDimension)
                .Method(method => method
                    .Name("hnsw")
                    .Engine("lucene")
                    .SpaceType("l2")
                    .Parameters(pm => pm
                        .Parameter("ef_construction", 128)
                        .Parameter("m", 24))
                )
            )
        );
    }
}
