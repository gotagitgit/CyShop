using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Principal;
using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace KeycloakClient;

internal class AdminTokenDelegatingHandler : DelegatingHandler
{
    private readonly string _tokenEndpoint;
    private readonly string _username;
    private readonly string _password;
    private string? _accessToken;

    public AdminTokenDelegatingHandler(IConfiguration configuration, string sectionName)
    {
        var section = configuration.GetSection(sectionName);
        var baseUrl = (section["auth-server-url"] ?? section["BaseUrl"] ?? "http://localhost:8080").TrimEnd('/');
        _tokenEndpoint = $"{baseUrl}/realms/master/protocol/openid-connect/token";
        _username = section["AdminUser"] ?? section["Username"] ?? "admin";
        _password = section["AdminPassword"] ?? section["Password"] ?? "admin";
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        _accessToken ??= await FetchTokenAsync(cancellationToken);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);

        var response = await base.SendAsync(request, cancellationToken);

        // If we get a 401, the token may have expired — refresh once and retry
        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            _accessToken = await FetchTokenAsync(cancellationToken);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);
            response = await base.SendAsync(request, cancellationToken);
        }

        return response;
    }

    private async Task<string> FetchTokenAsync(CancellationToken cancellationToken)
    {
        using var tokenClient = new HttpClient();
        var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["grant_type"] = "password",
            ["client_id"] = "admin-cli",
            ["username"] = _username,
            ["password"] = _password
        });

        var response = await tokenClient.PostAsync(_tokenEndpoint, content, cancellationToken);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadFromJsonAsync<JsonElement>(cancellationToken: cancellationToken);
        return json.GetProperty("access_token").GetString()!;
    }
}
