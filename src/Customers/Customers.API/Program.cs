using Customers.API.Endpoints;
using Customers.Application;
using Customers.Infrastructure;
using CyShop.ServiceDefaults;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddOpenApi();

builder.Services.AddApplicationServices();
builder.Services.AddCustomersInfrastructureServices(builder.Configuration);

builder.AddDefaultAuthentication();

builder.AddDefaultCors();

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

app.MapCustomerEndpoints();

app.Run();
