using System.ComponentModel;
using Chat.Domain.Entities;
using Chat.Domain.Interfaces;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using AiChatMessage = Microsoft.Extensions.AI.ChatMessage;
using AiChatRole = Microsoft.Extensions.AI.ChatRole;
using ChatMessage = Chat.Domain.Entities.ChatMessage;

namespace Chat.Infrastructure.Adapters;

public class OllamaChatCompletionAdapter : IChatCompletionService
{
    private readonly IChatClient _chatClient;
    private readonly ILogger<OllamaChatCompletionAdapter> _logger;
    private readonly ISearchCatalogService _searchService;
    private IReadOnlyList<ChatProduct> _lastSearchResults = [];

    private const string SystemPrompt = """
        You are a shopping assistant for CyShop, an online store that sells clothing and equipment for outdoor activities.
        RULES:
        - Use the SearchCatalog tool when the user asks about products, categories, or prices.
        - If products are already in the conversation from a previous search, answer from context without searching again.
        - When presenting products, always include their name and price.
        - If no relevant products are found, let the user know politely.
        - NEVER make up or invent product information. Only reference products returned by SearchCatalog.
        - If someone asks about something unrelated to CyShop, politely redirect them.
        """;

    public OllamaChatCompletionAdapter(
        IChatClient chatClient,
        ISearchCatalogService searchService,
        ILogger<OllamaChatCompletionAdapter> logger,
        IOptions<ChatSettings> settings)
    {
        _searchService = searchService;
        _logger = logger;

        // Wrap the chat client with function invocation support
        _chatClient = new ChatClientBuilder(chatClient)
            .UseFunctionInvocation()
            .Build();
    }

    public async Task<(string Answer, IReadOnlyList<ChatProduct> Products)> GetResponseAsync(
        IReadOnlyList<ChatMessage> history,
        string query,
        CancellationToken ct = default)
    {
        _lastSearchResults = [];

        var searchTool = AIFunctionFactory.Create(SearchCatalogAsync, "SearchCatalog",
            "Search the CyShop product catalog by natural language query. Returns product names and prices.");

        var messages = new List<AiChatMessage>
        {
            new(AiChatRole.System, SystemPrompt)
        };

        foreach (var msg in history)
        {
            var role = msg.Role == "user" ? AiChatRole.User : AiChatRole.Assistant;
            messages.Add(new AiChatMessage(role, msg.Content));
        }

        messages.Add(new AiChatMessage(AiChatRole.User, query));

        _logger.LogInformation("Sending {Count} messages to LLM with SearchCatalog tool", messages.Count);

        var options = new ChatOptions
        {
            Tools = [searchTool]
        };

        var response = await _chatClient.GetResponseAsync(messages, options, ct);
        var answer = response.Text ?? string.Empty;

        _logger.LogInformation("LLM response: {Answer}",
            answer.Length > 300 ? answer[..300] + "..." : answer);

        return (answer, _lastSearchResults);
    }

    [Description("Search the product catalog")]
    private async Task<string> SearchCatalogAsync(
        [Description("The search query")] string query,
        CancellationToken ct = default)
    {
        _logger.LogInformation("Tool called: SearchCatalog('{Query}')", query);
        var products = await _searchService.SearchAsync(query, ct);
        _lastSearchResults = products;

        if (products.Count == 0)
            return "No products found matching the query.";

        return string.Join("\n", products.Select(p => $"- {p.Name}: ${p.Price}"));
    }
}
