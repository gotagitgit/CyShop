using Catalog.API.Endpoints;
using Catalog.Application;
using Catalog.Infrastructure;
using Catalog.Infrastructure.Data;
using Scalar.AspNetCore;
using CyShop.ServiceDefaults;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddOpenApi();
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);

builder.AddDefaultCors();

var app = builder.Build();

// Only auto-create and seed in development
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
    await context.Database.EnsureCreatedAsync();
    var seeder = scope.ServiceProvider.GetRequiredService<DataSeeder>();
    await seeder.SeedAsync();
}

app.MapDefaultEndpoints();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}
    

app.UseCors();
app.UseHttpsRedirection();

app.MapCatalogEndpoints();

app.Run();
