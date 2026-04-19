using Catalog.Infrastructure;
using Customers.Infrastructure;
using CyShop.DbMigrator;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Auth.Infrastructure;
using Storage.Infrastructure;

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
services.AddScoped<MigrationRunner>();
services.AddScoped<AuthSeeder>();
services.AddSingleton<StorageSeeder>();

services.AddTransient<ClientCredentialsDelegatingHandler>();
services.AddHttpClient<CatalogApiSeeder>()
    .AddHttpMessageHandler<ClientCredentialsDelegatingHandler>();
services.AddHttpClient<CustomerApiSeeder>()
    .AddHttpMessageHandler<ClientCredentialsDelegatingHandler>();

await using var provider = services.BuildServiceProvider();
using var scope = provider.CreateScope();
var runner = scope.ServiceProvider.GetRequiredService<MigrationRunner>();
await runner.RunAllAsync();
