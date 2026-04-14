using Catalog.Infrastructure;
using CyShop.DbMigrator;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

var configuration = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: false)
    .Build();

var services = new ServiceCollection();
services.AddSingleton<IConfiguration>(configuration);
services.AddLogging(b => b.AddConsole());
services.AddInfrastructureServices(configuration);
services.AddScoped<MigrationRunner>();

// Future: services.AddOrdersInfrastructureServices(configuration);

await using var provider = services.BuildServiceProvider();
using var scope = provider.CreateScope();
var runner = scope.ServiceProvider.GetRequiredService<MigrationRunner>();
await runner.RunAllAsync();
