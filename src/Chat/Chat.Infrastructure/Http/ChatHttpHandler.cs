using Microsoft.Extensions.Logging;

namespace Chat.Infrastructure.Http;

public class ChatHttpHandler(ILoggerFactory loggerFactory) : DelegatingHandler
{
    private readonly ILogger _logger = loggerFactory.CreateLogger<ChatHttpHandler>();

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        if (request.Content is not null)
        {
            var requestBody = await request.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogInformation("Chat Request {Method} {Uri}\n{Body}",
                request.Method, request.RequestUri, requestBody);
        }

        var response = await base.SendAsync(request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogWarning("Chat Response {StatusCode}\n{Body}",
                (int)response.StatusCode, responseBody);
        }

        return response;
    }
}
