namespace Chat.Application.Services;

using Chat.Application.DTOs;
using Chat.Application.Interfaces;
using Chat.Domain.Entities;
using Chat.Domain.Interfaces;

public class ChatService(IChatCompletionService completionService) : IChatService
{
    public async Task<ChatResponse> ChatAsync(ChatRequest request, CancellationToken ct = default)
    {
        var history = request.Messages
            .Select(m => new ChatMessage(m.Role, m.Content))
            .ToList();

        var answer = await completionService.GetResponseAsync(history, request.Query, ct);

        return new ChatResponse(answer);
    }
}
