namespace Chat.Application.Interfaces;

using Chat.Application.DTOs;

public interface IChatService
{
    Task<ChatResponse> ChatAsync(ChatRequest request, CancellationToken ct = default);
}
