namespace Chat.Infrastructure;

using Chat.Domain.Interfaces;
using Chat.Infrastructure.Adapters;
using Chat.Infrastructure.Plugins;
using Chat.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.SemanticKernel;
using SearchServices;
using SearchServices.Settings;

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

        // Domain port: ISearchCatalogService → OpenSearchCatalogService
        services.AddScoped<ISearchCatalogService, OpenSearchCatalogService>();

        // Register SearchCatalogPlugin as scoped so it can consume scoped ISearchCatalogService
        services.AddScoped<SearchCatalogPlugin>();

        // Semantic Kernel + Ollama
        var kernelBuilder = services.AddKernel();
        kernelBuilder.AddOllamaChatCompletion(
            modelId: chatSection["ModelName"] ?? "llama3.2:1b",
            endpoint: new Uri(chatSection["OllamaEndpoint"] ?? "http://localhost:11434"));

        // Register the plugin as a scoped KernelPlugin so it resolves within request scope
        services.AddScoped(sp =>
            KernelPluginFactory.CreateFromObject(sp.GetRequiredService<SearchCatalogPlugin>(), "SearchCatalog"));

        // Domain port: IChatCompletionService → OllamaChatCompletionAdapter
        services.AddScoped<Chat.Domain.Interfaces.IChatCompletionService, OllamaChatCompletionAdapter>();

        return services;
    }
}
