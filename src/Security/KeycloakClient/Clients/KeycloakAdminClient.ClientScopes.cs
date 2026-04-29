using System.Net.Http.Json;
using System.Text.Json;
using Keycloak.AuthServices.Sdk.Admin.Models;

namespace KeycloakClient.Clients;

public partial class KeycloakAdminClient
{
    public async Task<JsonElement[]> GetClientScopesAsync(
        string realmName, CancellationToken ct = default)
    {
        var response = await _httpClient.GetAsync(
            $"/admin/realms/{realmName}/client-scopes", ct);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<JsonElement[]>(JsonOptions, ct)
            ?? [];
    }

    public async Task CreateClientScopeAsync(
        string realmName, string scopeName, CancellationToken ct = default)
    {
        var scope = new
        {
            name = scopeName,
            type = "default",
            protocol = "openid-connect",
            attributes = new Dictionary<string, string>
            {
                ["include.in.token.scope"] = "true",
                ["display.on.consent.screen"] = "false"
            }
        };

        var response = await _httpClient.PostAsJsonAsync(
            $"/admin/realms/{realmName}/client-scopes", scope, JsonOptions, ct);
        response.EnsureSuccessStatusCode();
    }

    public async Task<UserRepresentation> GetServiceAccountUserAsync(
        string realmName, string clientInternalId, CancellationToken ct = default)
    {
        var response = await _httpClient.GetAsync(
            $"/admin/realms/{realmName}/clients/{clientInternalId}/service-account-user", ct);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<UserRepresentation>(JsonOptions, ct)
            ?? throw new InvalidOperationException("Service account user not found.");
    }

    public async Task AddDefaultClientScopeAsync(
        string realmName, string clientInternalId, string scopeId, CancellationToken ct = default)
    {
        var response = await _httpClient.PutAsync(
            $"/admin/realms/{realmName}/clients/{clientInternalId}/default-client-scopes/{scopeId}", null, ct);
        response.EnsureSuccessStatusCode();
    }
}
