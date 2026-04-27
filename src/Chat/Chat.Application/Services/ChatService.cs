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

        var (answer, products) = await completionService.GetResponseAsync(history, request.Query, ct);

        var productDtos = products
            .Select(p => new ChatProductDto(p.Id, p.Name, p.Price))
            .ToList();

        return new ChatResponse(answer, productDtos, null);
    }
}
