using Chat.Infrastructure.Models;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Options;

namespace Chat.Infrastructure.Factory.ChatClient;

internal sealed class ChatClientFactory(
    IEnumerable<IChatAPIClientFactory> chatAPIClientFactories,
    IOptions<ChatSettings> options) : IChatClientFactory
{
    private readonly Dictionary<AIClient, IChatAPIClientFactory> _clientAPIFactory =
        chatAPIClientFactories.ToDictionary(x => x.ClientType, y => y);

    public IChatClient Create()
    {
        var settings = options.Value;

        if (!Enum.TryParse<AIClient>(settings.Provider, true, out var aiClient))
            throw new InvalidOperationException($"Unsupported AI client provider: {settings.Provider}");

        return _clientAPIFactory[aiClient].Create();
    }
}
