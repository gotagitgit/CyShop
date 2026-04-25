using Microsoft.Extensions.Logging;

namespace CyShop.DbMigrator;

public class OpenSearchSeeder(
    OpenSearchModelSetup modelSetup,
    ILogger<OpenSearchSeeder> logger)
{
    public async Task SeedAsync(CancellationToken ct = default)
    {
        logger.LogInformation("[OpenSearch] Starting setup...");
        await modelSetup.RunAsync(ct);
        logger.LogInformation("[OpenSearch] Setup completed.");
    }
}
