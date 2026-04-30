using Chat.Infrastructure.Models;
using Microsoft.Extensions.AI;

namespace Chat.Infrastructure.Factory.ChatClient;

internal interface IChatAPIClientFactory
{
    AIClient ClientType { get; }

    IChatClient Create();
}
