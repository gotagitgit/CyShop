using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CyShop.DbMigrator;

public sealed class ClientCredentialsDelegatingHandler(
    IConfiguration configuration,
    ILogger<ClientCredentialsDelegatingHandler> logger) : DelegatingHandler
{
    private string? _cachedToken;

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        if (_cachedToken is null)
            _cachedToken = await ObtainTokenAsync(cancellationToken);

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _cachedToken);

        var response = await base.SendAsync(request, cancellationToken);

        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            logger.LogInformation("Received 401, refreshing client credentials token");

            _cachedToken = await ObtainTokenAsync(cancellationToken);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _cachedToken);

            response = await base.SendAsync(request, cancellationToken);
        }

        return response;
    }

    private async Task<string> ObtainTokenAsync(CancellationToken cancellationToken)
    {
        var baseUrl = configuration["Keycloak:BaseUrl"] ?? "http://localhost:8080";
        var realm = configuration["Keycloak:Realm"] ?? "cyshop";
        var clientId = configuration["Keycloak:ServiceClientId"] ?? "cyshop-service";
        var clientSecret = configuration["Keycloak:ServiceClientSecret"] ?? "service-secret";

        var tokenEndpoint = $"{baseUrl}/realms/{realm}/protocol/openid-connect/token";

        logger.LogInformation("Requesting client credentials token from {Endpoint}", tokenEndpoint);

        using var httpClient = new HttpClient();
        var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["grant_type"] = "client_credentials",
            ["client_id"] = clientId,
            ["client_secret"] = clientSecret
        });

        var response = await httpClient.PostAsync(tokenEndpoint, content, cancellationToken);
        response.EnsureSuccessStatusCode();

        var tokenResponse = await response.Content.ReadFromJsonAsync<TokenResponse>(cancellationToken);

        return tokenResponse?.AccessToken
            ?? throw new InvalidOperationException("Token response did not contain an access token");
    }

    private sealed class TokenResponse
    {
        [JsonPropertyName("access_token")]
        public string? AccessToken { get; set; }
    }
}
