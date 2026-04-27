var builder = DistributedApplication.CreateBuilder(args);

// External resources managed by docker-compose
var redis = builder.AddConnectionString("basketcache");
var keycloak = builder.AddConnectionString("keycloak");
var storage = builder.AddConnectionString("storage");

var catalogApi = builder.AddProject<Projects.Catalog_API>("catalog-api")
    .WithReference(storage);

var basketApi = builder.AddProject<Projects.Basket_API>("basket-api")
       .WithReference(redis)
       .WithReference(keycloak);

var chatApi = builder.AddProject<Projects.Chat_API>("chat-api");

builder.AddViteApp("cyshop-web", "../../CyShop.Web")
       .WithReference(catalogApi)
       .WithReference(basketApi)
       .WithReference(chatApi);

builder.AddProject<Projects.Customers_API>("customers-api");

builder.AddProject<Projects.Orders_API>("orders-api");

builder.Build().Run();
