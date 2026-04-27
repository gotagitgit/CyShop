using Chat.API.Endpoints;
using Chat.Application;
using Chat.Infrastructure;
using CyShop.ServiceDefaults;
using SearchServices;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddOpenApi();
builder.Services.AddApplicationServices();
builder.AddInfrastructureServices();
builder.Services.AddSearchServices();

builder.AddDefaultCors();
builder.AddDefaultAuthentication();

var app = builder.Build();

app.MapDefaultEndpoints();
app.UseCors();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapChatEndpoints();
app.Run();
