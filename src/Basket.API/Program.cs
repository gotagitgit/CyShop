using Basket.API.Endpoints;
using Basket.API.IntegrationEvents.EventHandling;
using Basket.API.IntegrationEvents.Events;
using Basket.API.Repositories;
using CyShop.ServiceDefaults;
using EventBus.Abstractions;
using RabbitMQEventBus;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddOpenApi();

// Register Redis connection via Aspire
builder.AddRedisClient("basketcache");

// Register basket repository
builder.Services.AddScoped<IBasketRepository, RedisBasketRepository>();

builder.AddDefaultAuthentication();

builder.AddDefaultCors();

builder.AddRabbitMqEventBus("eventbus")
    .AddSubscription<OrderStartedIntegrationEvent, OrderStartedIntegrationEventHandler>();

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
