namespace Chat.API.Endpoints;

using Chat.Application.DTOs;
using Chat.Application.Interfaces;
using Microsoft.Extensions.Logging;

public static class ChatEndpoints
{
    public static WebApplication MapChatEndpoints(this WebApplication app)
    {
        app.MapPost("/api/chat", HandleChat)
           .RequireAuthorization();

        return app;
    }

    private static async Task<IResult> HandleChat(
        ChatRequest request,
        IChatService chatService,
        ILogger<ChatRequest> logger,
        CancellationToken ct)
    {
        if (request.Messages is null || string.IsNullOrWhiteSpace(request.Query))
            return Results.BadRequest(new { error = "Messages array and non-empty query are required." });

        try
        {
            var response = await chatService.ChatAsync(request, ct);
            return Results.Ok(response);
        }
        catch (TimeoutException ex)
        {
            logger.LogError(ex, "Chat request timed out");
            return Results.Problem("Chat service request timed out.", statusCode: 500);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Chat request failed");
            return Results.Problem("Chat service is temporarily unavailable.", statusCode: 500);
        }
    }
}
