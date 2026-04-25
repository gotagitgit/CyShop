using Orders.API.Endpoints;
using Orders.API.Middleware;
using Orders.Application;
using Orders.Infrastructure;
using Orders.Infrastructure.Data;
using CyShop.ServiceDefaults;
using Microsoft.EntityFrameworkCore;
using RabbitMQEventBus;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddOpenApi();
builder.Services.AddApplicationServices();
builder.Services.AddOrdersInfrastructureServices(builder.Configuration);

builder.AddRabbitMqEventBus("eventbus");

builder.AddDefaultCors();

builder.AddDefaultAuthentication();
builder.Services.AddCurrentUser();

var app = builder.Build();

app.MapDefaultEndpoints();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors();
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.UseCurrentUser();

app.UseTransactionMiddleware();

app.MapOrderEndpoints();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<OrdersDbContext>();
    db.Database.Migrate();
}

app.Run();
