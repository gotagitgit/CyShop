namespace Chat.Application.DTOs;

public sealed record ChatResponse(
    string Answer,
    IReadOnlyList<ChatProductDto> Products,
    SuggestedActionDto? SuggestedAction);
