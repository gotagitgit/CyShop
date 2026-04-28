using Auth.Infrastructure;
using Catalog.Infrastructure;
using Customers.Infrastructure;
using Cyshop.Common.Models;
using CyShop.Common.Http;
using CyShop.DbMigrator;
using CyShop.DbMigrator.Models;
using CyShop.ServiceDefaults;
using CyShop.ServiceDefaults.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Orders.Infrastructure;
using SearchServices;
using SearchServices.Settings;
using StackExchange.Redis;
using Storage.Infrastructure;

var options = CommandLineOptions.Parse(args);

var configuration = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: false)
    .Build();

var services = new ServiceCollection();
services.AddSingleton<IConfiguration>(configuration);
services.AddLogging(b => b.AddConsole());
services.AddInfrastructureServices(configuration);
services.AddCustomersInfrastructureServices(configuration);
services.AddAuthInfrastructure(configuration);
services.AddStorageInfrastructure(configuration);
services.AddOrdersInfrastructureServices(configuration);
services.Configure<SearchSettings>(s =>
{
    s.SearchAddress = configuration["OpenSearch:Endpoint"] ?? "http://localhost:9200";
    s.IngestPipeline = configuration["OpenSearch:IngestPipeline"] ?? "catalog-neural-pipeline";
    s.EmbeddingDimension = int.Parse(configuration["OpenSearch:EmbeddingDimension"] ?? "384");
});
services.AddSearchServices();
services.AddSingleton(options);
services.AddScoped<MigrationRunner>();
services.AddScoped<AuthSeeder>();
services.AddSingleton<StorageSeeder>();
services.AddHttpClient<OpenSearchSeeder>();
services.AddScoped<OpenSearchModelSetup>();
services.AddSingleton<DevUser>();
services.AddSingleton<ICurrentUser>(sp => sp.GetRequiredService<DevUser>());

var redisConnectionString = configuration.GetConnectionString("Redis") ?? "localhost:6379";
services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect($"{redisConnectionString},allowAdmin=true"));

services.AddSingleton<IHttpContextAccessor>(sp =>
{
    var context = new DefaultHttpContext { RequestServices = sp };
    return new ScopedHttpContextAccessor(context);
});
services.AddScoped<ClientCredentialsDelegatingHandler>();
services.AddHttpClient<CatalogApiSeeder>()
    .AddHttpMessageHandler<ClientCredentialsDelegatingHandler>();
services.AddHttpClient<CustomerApiSeeder>()
    .AddHttpMessageHandler<ClientCredentialsDelegatingHandler>();

await using var provider = services.BuildServiceProvider();
using var scope = provider.CreateScope();
var runner = scope.ServiceProvider.GetRequiredService<MigrationRunner>();
await runner.RunAllAsync();

/// <summary>
/// IHttpContextAccessor for non-web hosts that exposes the IServiceProvider
/// via a minimal HttpContext, so that DelegatingHandlers can resolve services
/// (e.g. ICurrentUser → DevUser) the same way they do in web hosts.
/// </summary>
internal sealed class ScopedHttpContextAccessor(HttpContext context) : IHttpContextAccessor
{
    public HttpContext? HttpContext { get; set; } = context;
}
