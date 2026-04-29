using Chat.Domain.Interfaces;
using Chat.Infrastructure.Factory;
using Chat.Infrastructure.Services;
using Chat.Infrastructure.Tools;
using CyShop.Common.Http;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using SearchServices;
using SearchServices.Settings;

namespace Chat.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(
        this IHostApplicationBuilder builder)
    {
        var services = builder.Services;
        var config = builder.Configuration;
        
        var chatSection = config.GetSection(nameof(ChatSettings));
        services.Configure<ChatSettings>(chatSection);
        services.Configure<SearchSettings>(config.GetSection(nameof(SearchSettings)))
                .AddSearchServices();

        RegisterHttpClients(services, config);
        RegisterChatClient(services);
        RegisterChatTools(services);

        services.AddScoped<IChatHttpClientFactory, ChatHttpClientFactory>();
        services.AddScoped<IChatCompletionService, OllamaChatCompletionService>();

        return services;
    }

    private static void RegisterHttpClients(IServiceCollection services, IConfigurationManager config)
    {
        services.AddScoped<ClientCredentialsDelegatingHandler>();
        services.AddHttpClient(ChatHttpClientFactory.BasketApiClientName, client =>
        {
            client.BaseAddress = new Uri(config["ApiEndpoints:BasketApi"] ?? "http://localhost:5167");
        }).AddHttpMessageHandler<ClientCredentialsDelegatingHandler>();
    }

    private static void RegisterChatTools(IServiceCollection services)
    {
        services.AddScoped<IChatTool, AddToBasketTool>();
        services.AddScoped<IChatTool, SearchCatalogTool>();
    }

    private static void RegisterChatClient(IServiceCollection services)
    {
        services.AddSingleton<IChatClient>(sp =>
        {
            var settings = sp.GetRequiredService<IOptions<ChatSettings>>().Value;
            var httpClient = new HttpClient
            {
                BaseAddress = new Uri(settings.OllamaEndpoint),
                Timeout = TimeSpan.FromSeconds(settings.TimeoutSeconds)
            };

            return new OllamaSharp.OllamaApiClient(httpClient, settings.ModelName);
        });
    }
}
