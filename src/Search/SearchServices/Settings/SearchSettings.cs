namespace SearchServices.Settings;

public class SearchSettings
{
    public string SearchAddress { get; set; } = string.Empty;
    public string IngestPipeline { get; set; } = "catalog-neural-pipeline";
    public string SearchPipeline { get; set; } = "catalog-hybrid-search";
    public int EmbeddingDimension { get; set; } = 384;
}
