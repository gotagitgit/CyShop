namespace SearchServices.Settings;

public class SearchSettings
{
    public string SearchAddress { get; set; } = string.Empty;
    public string IngestPipeline { get; set; } = "catalog-neural-pipeline";
    public int EmbeddingDimension { get; set; } = 384;
}
