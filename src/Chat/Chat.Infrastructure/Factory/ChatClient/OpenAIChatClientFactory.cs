using System.ClientModel;
using Chat.Infrastructure.Http;
using Chat.Infrastructure.Models;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenAI;

namespace Chat.Infrastructure.Factory.ChatClient;

internal sealed class OpenAIChatClientFactory(
    IOptions<ChatSettings> options,
    ILoggerFactory loggerFactory) : IChatAPIClientFactory
{
    public AIClient ClientType => AIClient.OpenAI;

    public IChatClient Create()
    {
        var settings = options.Value;

        var handler = new ChatHttpHandler(loggerFactory);
        var httpClient = new HttpClient(handler)
        {
            Timeout = TimeSpan.FromSeconds(settings.TimeoutSeconds)
        };

        var credential = new ApiKeyCredential(settings.HuggingFaceApiKey);
        var clientOptions = new OpenAIClientOptions
        {
            Endpoint = new Uri(settings.HuggingFaceEndpoint),
            Transport = new System.ClientModel.Primitives.HttpClientPipelineTransport(httpClient)
        };

        return new OpenAI.Chat.ChatClient(settings.HuggingFaceModelName, credential, clientOptions)
            .AsIChatClient();
    }
}
