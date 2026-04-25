using Catalog.Infrastructure;
using Customers.Infrastructure;
using CyShop.DbMigrator;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Auth.Infrastructure;
using Storage.Infrastructure;
using Orders.Infrastructure;
using SearchServices;
using SearchServices.Settings;
using StackExchange.Redis;

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
    s.SearchAddress = configuration["OpenSearch:Endpoint"] ?? "http://localhost:9200");
services.Register();
services.AddSingleton(options);
services.AddScoped<MigrationRunner>();
services.AddScoped<AuthSeeder>();
services.AddSingleton<StorageSeeder>();
services.AddScoped<OpenSearchSeeder>();
services.AddScoped<OpenSearchModelSetup>();

var redisConnectionString = configuration.GetConnectionString("Redis") ?? "localhost:6379";
services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect($"{redisConnectionString},allowAdmin=true"));

services.AddTransient<ClientCredentialsDelegatingHandler>();
services.AddHttpClient<CatalogApiSeeder>()
    .AddHttpMessageHandler<ClientCredentialsDelegatingHandler>();
services.AddHttpClient<CustomerApiSeeder>()
    .AddHttpMessageHandler<ClientCredentialsDelegatingHandler>();

await using var provider = services.BuildServiceProvider();
using var scope = provider.CreateScope();
var runner = scope.ServiceProvider.GetRequiredService<MigrationRunner>();
await runner.RunAllAsync();
