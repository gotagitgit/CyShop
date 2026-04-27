namespace Chat.Domain.Interfaces;

using Chat.Domain.Entities;

public interface IChatCompletionService
{
    Task<(string Answer, IReadOnlyList<ChatProduct> Products)> GetResponseAsync(
        IReadOnlyList<ChatMessage> history,
        string query,
        CancellationToken ct = default);
}
