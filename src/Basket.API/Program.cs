using Basket.API.Endpoints;
using Basket.API.Repositories;
using CyShop.ServiceDefaults;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddOpenApi();

// Register Redis connection via Aspire
builder.AddRedisClient("basketcache");

// Register basket repository
builder.Services.AddScoped<IBasketRepository, RedisBasketRepository>();

builder.AddDefaultAuthentication();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod());
});

var app = builder.Build();

app.MapDefaultEndpoints();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseCors();
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapBasketEndpoints();

app.Run();
