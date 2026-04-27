namespace Chat.Application.DTOs;

public sealed record ChatRequest(
    IReadOnlyList<ChatMessageDto> Messages,
    string Query);
