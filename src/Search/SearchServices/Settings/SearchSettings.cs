namespace SearchServices.Settings;

public class SearchSettings
{
    public string SearchAddress { get; set; } = string.Empty;
    public string IngestPipeline { get; set; } = "catalog-neural-pipeline";
    public string SearchPipeline { get; set; } = "catalog-hybrid-search";
    public string ModelName { get; set; } = "huggingface/sentence-transformers/all-MiniLM-L6-v2";
    public string ModelGroupName { get; set; } = "catalog_model_group";
    public int EmbeddingDimension { get; set; } = 384;
}
