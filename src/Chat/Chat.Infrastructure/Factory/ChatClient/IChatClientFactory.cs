using Microsoft.Extensions.AI;

namespace Chat.Infrastructure.Factory.ChatClient;

public interface IChatClientFactory
{
    IChatClient Create();
}
