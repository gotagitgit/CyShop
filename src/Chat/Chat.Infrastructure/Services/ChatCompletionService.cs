using Chat.Domain.Interfaces;
using Chat.Infrastructure.Factory.ChatClient;
using Chat.Infrastructure.Tools;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using ChatMessage = Microsoft.Extensions.AI.ChatMessage;
using ChatRole = Microsoft.Extensions.AI.ChatRole;
using DomainChatMessage = Chat.Domain.Entities.ChatMessage;

namespace Chat.Infrastructure.Services;

internal class ChatCompletionService : IChatCompletionService
{
    private readonly IChatClient _chatClient;
    private readonly IEnumerable<IChatTool> _tools;
    private readonly ILogger<ChatCompletionService> _logger;

    private const string SystemPrompt = """
        You are a shopping assistant for CyShop, an online store that sells clothing and equipment for outdoor activities.
        RULES:
        - Use SearchCatalog when the user asks about products, categories, or prices, or wants to browse/find items.
        - Use AddToBasket when the user wants to add a product to their cart/basket. You MUST already know the product's productId, productName, and unitPrice from a previous search result. Do NOT call SearchCatalog first if you already have the product details in the conversation.
        - If products are already in the conversation from a previous search, answer from context without searching again.
        - When presenting products, always include their name and price.
        - If no relevant products are found, let the user know politely.
        - NEVER make up or invent product information. Only reference products returned by tools.
        - If someone asks about something unrelated to CyShop, politely redirect them.
        """;

    public ChatCompletionService(
        IChatClientFactory chatClientFactory,
        IEnumerable<IChatTool> tools,
        ILogger<ChatCompletionService> logger)
    {
        _tools = tools;
        _logger = logger;

        var chatClient = chatClientFactory.Create();

        _chatClient = new ChatClientBuilder(chatClient)
            .UseFunctionInvocation()
            .Build();
    }

    public async Task<string> GetResponseAsync(
        IReadOnlyList<DomainChatMessage> history,
        string query,
        CancellationToken ct = default)
    {
        var aiFunctions = _tools.Select(t => t.Create()).ToList();

        var messages = new List<ChatMessage>
        {
            new(ChatRole.System, SystemPrompt)
        };

        foreach (var msg in history)
        {
            var role = msg.Role == "user" ? ChatRole.User : ChatRole.Assistant;
            messages.Add(new ChatMessage(role, msg.Content));
        }

        messages.Add(new ChatMessage(ChatRole.User, query));

        _logger.LogInformation("Sending {Count} messages to LLM", messages.Count);

        var options = new ChatOptions
        {
            Tools = [.. aiFunctions]
        };

        var response = await _chatClient.GetResponseAsync(messages, options, ct);
        var answer = response.Text ?? string.Empty;

        _logger.LogInformation("LLM response: {Answer}",
            answer.Length > 300 ? answer[..300] + "..." : answer);

        return answer;
    }
}
