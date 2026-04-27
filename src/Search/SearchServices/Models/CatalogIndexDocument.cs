using System.Text.Json.Serialization;

namespace SearchServices.Models;

public sealed record CatalogIndexDocument
{
    [JsonPropertyName("id")]
    public string Id { get; init; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; init; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; init; } = string.Empty;

    [JsonPropertyName("nameDescription")]
    public string NameDescription { get; init; } = string.Empty;

    [JsonPropertyName("price")]
    public decimal Price { get; init; }

    [JsonPropertyName("brandName")]
    public string BrandName { get; init; } = string.Empty;

    [JsonPropertyName("typeName")]
    public string TypeName { get; init; } = string.Empty;
}
