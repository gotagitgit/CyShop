namespace Chat.Infrastructure.Adapters;

using Chat.Domain.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

public class OllamaChatCompletionAdapter(
    Kernel kernel,
    Microsoft.SemanticKernel.ChatCompletion.IChatCompletionService skChatCompletion,
    IEnumerable<KernelPlugin> plugins,
    ILogger<OllamaChatCompletionAdapter> logger,
    IOptions<ChatSettings> settings) : Chat.Domain.Interfaces.IChatCompletionService
{
    private const string SystemPrompt = """
        You are a shopping assistant for CyShop, an online store. You MUST use the SearchCatalog function to find products.
        RULES:
        - ALWAYS call SearchCatalog when the user asks about products, categories, prices, or anything related to shopping.
        - NEVER make up or invent product information. Only reference products returned by SearchCatalog.
        - If products are already in the conversation context from a previous SearchCatalog call, you may answer from context.
        - When presenting products, include their name and price.
        - If SearchCatalog returns no results, tell the user politely that no matching products were found.
        - Do NOT provide generic advice or lists. You are a store assistant, not a general knowledge bot.
        """;

    public async Task<(string Answer, IReadOnlyList<ChatProduct> Products)> GetResponseAsync(
        IReadOnlyList<ChatMessage> history,
        string query,
        CancellationToken ct = default)
    {
        // Add scoped plugins to the kernel so the LLM can discover them
        foreach (var plugin in plugins)
        {
            if (!kernel.Plugins.Contains(plugin))
                kernel.Plugins.Add(plugin);
        }

        // Log registered plugins and their functions
        logger.LogInformation("Kernel has {PluginCount} plugins registered", kernel.Plugins.Count);
        foreach (var p in kernel.Plugins)
        {
            var funcNames = string.Join(", ", p.Select(f => f.Name));
            logger.LogInformation("  Plugin '{PluginName}': [{Functions}]", p.Name, funcNames);
        }

        var chatHistory = new ChatHistory(SystemPrompt);

        foreach (var msg in history)
        {
            if (msg.Role == "user")
                chatHistory.AddUserMessage(msg.Content);
            else if (msg.Role == "assistant")
                chatHistory.AddAssistantMessage(msg.Content);
        }

        chatHistory.AddUserMessage(query);

        // Log what we're sending
        logger.LogInformation("Sending chat with {MessageCount} messages, query: '{Query}'", chatHistory.Count, query);

        var executionSettings = new PromptExecutionSettings
        {
            FunctionChoiceBehavior = FunctionChoiceBehavior.Required()
        };
        executionSettings.ExtensionData ??= new Dictionary<string, object>();
        executionSettings.ExtensionData["enable_thinking"] = false;

        var result = await skChatCompletion.GetChatMessageContentAsync(
            chatHistory, executionSettings, kernel, ct);

        // Log the full chat history after completion (includes function calls/results)
        logger.LogInformation("Chat completed. History now has {Count} messages", chatHistory.Count);
        foreach (var msg in chatHistory)
        {
            logger.LogInformation("  [{Role}] {Content}", msg.Role, msg.Content?.Length > 200 ? msg.Content[..200] + "..." : msg.Content);
        }

        logger.LogInformation("Final answer: {Answer}", result.Content?.Length > 300 ? result.Content[..300] + "..." : result.Content);
        
        var products = ExtractProducts(chatHistory);

        return (result.Content ?? string.Empty, products);
    }

    private static IReadOnlyList<ChatProduct> ExtractProducts(ChatHistory chatHistory)
    {
        return [];
    }
}
