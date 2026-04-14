var builder = DistributedApplication.CreateBuilder(args);

// External resources managed by docker-compose
var redis = builder.AddConnectionString("basketcache");
var keycloak = builder.AddConnectionString("keycloak");

builder.AddProject<Projects.Catalog_API>("catalog-api");

builder.AddProject<Projects.Basket_API>("basket-api")
       .WithReference(redis)
       .WithReference(keycloak);

builder.Build().Run();
