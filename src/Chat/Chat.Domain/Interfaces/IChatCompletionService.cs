namespace Chat.Domain.Interfaces;

using Chat.Domain.Entities;

public interface IChatCompletionService
{
    Task<string> GetResponseAsync(
        IReadOnlyList<ChatMessage> history,
        string query,
        CancellationToken ct = default);
}
