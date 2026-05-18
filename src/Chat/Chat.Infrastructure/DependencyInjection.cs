using Chat.Domain.Interfaces;
using Chat.Infrastructure.Factory;
using Chat.Infrastructure.Factory.ChatClient;
using Chat.Infrastructure.Services;
using Chat.Infrastructure.Tools;
using CyShop.Common.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
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

        services.Configure<ChatSettings>(config.GetSection(nameof(ChatSettings)));
        services.Configure<SearchSettings>(config.GetSection(nameof(SearchSettings)))
                .AddSearchServices();

        RegisterHttpClients(services, config);
        RegisterChatTools(services);
        RegisterFactories(services);

        services.AddScoped<IChatCompletionService, ChatCompletionService>();

        return services;
    }

    private static void RegisterFactories(IServiceCollection services)
    {
        services.AddScoped<IChatHttpClientFactory, ChatHttpClientFactory>();
        services.AddScoped<IChatAPIClientFactory, OpenAIChatClientFactory>();
        services.AddScoped<IChatAPIClientFactory, OllamaChatClientFactory>();
        services.AddScoped<IChatClientFactory, ChatClientFactory>();
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
}
