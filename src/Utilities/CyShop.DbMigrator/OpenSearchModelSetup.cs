using System.Security.Cryptography;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SearchServices.Models;
using SearchServices.Services;

namespace CyShop.DbMigrator;

public class OpenSearchModelSetup(
    IOpenSearchModelService modelService,
    IOpenSearchClusterService clusterService,
    IConfiguration configuration,
    ILogger<OpenSearchModelSetup> logger)
{
    public async Task RunAsync(CancellationToken ct = default)
    {
        var modelId = configuration["OpenSearch:ModelId"] ?? "huggingface/sentence-transformers/all-MiniLM-L6-v2";
        var embeddingDimension = int.Parse(configuration["OpenSearch:EmbeddingDimension"] ?? "384");
        var pipelineName = configuration["OpenSearch:IngestPipeline"] ?? "catalog-neural-pipeline";
        var searchPipelineName = configuration["OpenSearch:SearchPipeline"] ?? "catalog-hybrid-search";
        var modelFilePath = configuration["OpenSearch:ModelFile"]
            ?? Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..",
                "data", "opensearch",
                "sentence-transformers_all-MiniLM-L6-v2-1.0.1-torch_script.zip");

        // Step 1: Wait for cluster
        await WaitForClusterAsync(ct);

        // Step 2: Configure cluster settings
        logger.LogInformation("[OpenSearch] Configuring cluster settings...");
        var settingsResult = await clusterService.UpdateSettingsAsync(new ClusterSettings(new Dictionary<string, object>
        {
            ["plugins.ml_commons.only_run_on_ml_node"] = "false",
            ["plugins.ml_commons.model_access_control_enabled"] = "true",
            ["plugins.ml_commons.native_memory_threshold"] = "99",
            ["plugins.ml_commons.allow_registering_model_via_url"] = true
        }), ct);

        if (!settingsResult.IsSuccess)
            throw new InvalidOperationException($"Failed to configure cluster settings: {settingsResult.Body}");

        // Step 3: Create model group
        logger.LogInformation("[OpenSearch] Creating model group...");
        var groupResult = await modelService.CreateModelGroupAsync(
            new MLModelGroup("catalog_model_group", "Model group for catalog embedding models"), ct);

        if (!groupResult.IsSuccess)
            throw new InvalidOperationException("Failed to create or find model group.");
        logger.LogInformation("[OpenSearch] Model group ready (group_id: {GroupId})", groupResult.Id);

        // Step 4: Register model
        var deployedModelId = await RegisterModelAsync(modelId, embeddingDimension, modelFilePath, groupResult.Id, ct);

        // Step 5: Deploy model
        await DeployModelAsync(deployedModelId, ct);

        // Step 6: Create ingest pipeline
        logger.LogInformation("[OpenSearch] Ensuring ingest pipeline '{Pipeline}'...", pipelineName);
        var ingestResult = await clusterService.EnsureIngestPipelineAsync(
            new IngestPipelineConfig(pipelineName, deployedModelId, "nameDescription", "embedding"), ct);

        if (!ingestResult.IsSuccess)
            logger.LogWarning("[OpenSearch] Ingest pipeline failed: {Body}", ingestResult.Body);

        // Step 7: Create search pipeline
        logger.LogInformation("[OpenSearch] Ensuring search pipeline '{Pipeline}'...", searchPipelineName);
        var searchResult = await clusterService.EnsureSearchPipelineAsync(
            new SearchPipelineConfig(searchPipelineName, "min_max", "arithmetic_mean", [0.7, 0.3]), ct);

        if (!searchResult.IsSuccess)
            logger.LogWarning("[OpenSearch] Search pipeline failed: {Body}", searchResult.Body);

        logger.LogInformation("[OpenSearch] Model setup completed.");
    }

    private async Task WaitForClusterAsync(CancellationToken ct)
    {
        logger.LogInformation("[OpenSearch] Waiting for cluster...");
        var delay = TimeSpan.FromSeconds(1);
        var maxDelay = TimeSpan.FromSeconds(30);
        const int maxRetries = 10;

        for (var attempt = 1; attempt <= maxRetries; attempt++)
        {
            try
            {
                if (await clusterService.CheckHealthAsync(ct))
                {
                    logger.LogInformation("[OpenSearch] Cluster available.");
                    return;
                }
            }
            catch
            {
                // retry
            }

            if (attempt < maxRetries)
            {
                logger.LogInformation("[OpenSearch] Cluster not ready (attempt {Attempt}/{Max})", attempt, maxRetries);
                await Task.Delay(delay, ct);
                delay = TimeSpan.FromSeconds(Math.Min(delay.TotalSeconds * 2, maxDelay.TotalSeconds));
            }
        }

        throw new InvalidOperationException($"OpenSearch was not available after {maxRetries} retries");
    }

    private async Task<string> RegisterModelAsync(
        string modelId, int embeddingDimension, string modelFilePath,
        string modelGroupId, CancellationToken ct)
    {
        logger.LogInformation("[OpenSearch] Registering model '{ModelId}'...", modelId);

        if (!File.Exists(modelFilePath))
            throw new FileNotFoundException($"Model file not found: {Path.GetFullPath(modelFilePath)}");

        var fileName = Path.GetFileName(modelFilePath);
        var hash = ComputeSha256(modelFilePath);

        var registration = new MLModelRegistration(
            Name: modelId,
            Version: "1.0.1",
            ModelFormat: "TORCH_SCRIPT",
            ModelGroupId: modelGroupId,
            ModelType: "bert",
            EmbeddingDimension: embeddingDimension,
            FrameworkType: "sentence_transformers",
            Url: $"file:///tmp/models/{fileName}",
            ModelContentHashValue: hash);

        var result = await modelService.RegisterModelAsync(registration, ct);
        if (!result.IsSuccess)
            throw new InvalidOperationException("Failed to register model.");

        if (result.AlreadyExists)
        {
            logger.LogInformation("[OpenSearch] Model already registered (model_id: {ModelId})", result.ModelId);
            return result.ModelId!;
        }

        logger.LogInformation("[OpenSearch] Model registration submitted (task_id: {TaskId})", result.TaskId);
        var task = await modelService.PollTaskAsync(result.TaskId!, ct);
        logger.LogInformation("[OpenSearch] Model registered (model_id: {ModelId})", task.ModelId);
        return task.ModelId;
    }

    private async Task DeployModelAsync(string deployedModelId, CancellationToken ct)
    {
        logger.LogInformation("[OpenSearch] Deploying model...");
        var deployResult = await modelService.DeployModelAsync(deployedModelId, ct);
        if (!deployResult.IsSuccess)
            throw new InvalidOperationException("Failed to deploy model.");

        if (!string.IsNullOrEmpty(deployResult.TaskId))
        {
            await modelService.PollTaskAsync(deployResult.TaskId, ct);
            logger.LogInformation("[OpenSearch] Model deployed.");
        }
        else
        {
            logger.LogInformation("[OpenSearch] Model already deployed.");
        }
    }

    private static string ComputeSha256(string filePath)
    {
        using var stream = File.OpenRead(filePath);
        var hash = SHA256.HashData(stream);
        return Convert.ToHexStringLower(hash);
    }
}
