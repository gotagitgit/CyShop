using OpenSearch.Client;

namespace SearchServices.Factory;

public interface IIndexMappingFactory<T> where T : class
{
    ITypeMapping Create(TypeMappingDescriptor<T> descriptor);
}
