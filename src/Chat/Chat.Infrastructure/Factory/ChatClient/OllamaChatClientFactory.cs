using Chat.Infrastructure.Models;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Options;

namespace Chat.Infrastructure.Factory.ChatClient;

internal sealed class OllamaChatClientFactory(IOptions<ChatSettings> options) : IChatAPIClientFactory
{
    public AIClient ClientType => AIClient.Ollama;

    public IChatClient Create()
    {
        var settings = options.Value;

        var httpClient = new HttpClient
        {
            BaseAddress = new Uri(settings.OllamaEndpoint),
            Timeout = TimeSpan.FromSeconds(settings.TimeoutSeconds)
        };

        return new OllamaSharp.OllamaApiClient(httpClient, settings.OllamaModelName);
    }
}
