var builder = DistributedApplication.CreateBuilder(args);

// External resources managed by docker-compose
var redis = builder.AddConnectionString("basketcache");
var keycloak = builder.AddConnectionString("keycloak");

var catalogApi = builder.AddProject<Projects.Catalog_API>("catalog-api");

var basketApi = builder.AddProject<Projects.Basket_API>("basket-api")
       .WithReference(redis)
       .WithReference(keycloak);

builder.AddViteApp("cyshop-web", "../CyShop.Web")
       .WithReference(catalogApi)
       .WithReference(basketApi);

builder.Build().Run();
