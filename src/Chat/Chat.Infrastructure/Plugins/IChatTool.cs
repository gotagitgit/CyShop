namespace Chat.Infrastructure.Plugins;

using Microsoft.Extensions.AI;

/// <summary>
/// Represents a tool the LLM can invoke during chat.
/// Register implementations in DI to make them available to the chat adapter.
/// Future tools: AddToCart, GetOrderStatus, GetUserInfo, etc.
/// </summary>
public interface IChatTool
{
    AIFunction Create();
}
