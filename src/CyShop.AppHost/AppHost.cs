var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.Catalog_API>("catalog-api");

builder.AddProject<Projects.Basket_API>("basket-api");

builder.Build().Run();
