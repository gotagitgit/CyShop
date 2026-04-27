using Chat.Domain.Interfaces;
using Chat.Infrastructure.Plugins;
using Chat.Infrastructure.Services;
using Microsoft.Extensions.AI;
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

        // Chat settings
        var chatSection = config.GetSection("Chat");
        services.Configure<ChatSettings>(chatSection);

        // OpenSearch services from SearchServices library
        services.Configure<SearchSettings>(config.GetSection("Search"));
        services.AddSearchServices();

        // Ollama IChatClient — singleton, stateless HTTP client wrapper
        var timeoutSeconds = int.TryParse(chatSection["TimeoutSeconds"], out var t) ? t : 120;
        var ollamaEndpoint = chatSection["OllamaEndpoint"] ?? "http://localhost:11434";
        var ollamaModel = chatSection["ModelName"] ?? "qwen3.5:9b";

        var httpClient = new HttpClient
        {
            BaseAddress = new Uri(ollamaEndpoint),
            Timeout = TimeSpan.FromSeconds(timeoutSeconds)
        };
        services.AddSingleton<IChatClient>(new OllamaSharp.OllamaApiClient(httpClient, ollamaModel));

        // Chat tools — add new IChatTool implementations here for future capabilities
        services.AddScoped<IChatTool, SearchCatalogTool>();
        // services.AddScoped<IChatTool, AddToCartTool>();

        // Domain port: IChatCompletionService → OllamaChatCompletionService (scoped — one per request)
        services.AddScoped<IChatCompletionService, OllamaChatCompletionService>();

        return services;
    }
}
