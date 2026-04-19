using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Keycloak.AuthServices.Sdk.Admin;
using Keycloak.AuthServices.Sdk.Admin.Models;

namespace KeycloakClient.Clients;

public partial class KeycloakAdminClient : IKeycloakAdminClient
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private readonly IKeycloakClient _sdk;
    private readonly HttpClient _httpClient;

    public KeycloakAdminClient(HttpClient httpClient, IKeycloakClient sdk)
    {
        _httpClient = httpClient;
        _sdk = sdk;
    }

    public async Task<RealmRepresentation?> GetRealmAsync(
        string realmName, CancellationToken ct = default)
    {
        var response = await _sdk.GetRealmWithResponseAsync(realmName, ct);

        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            return null;

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<RealmRepresentation>(JsonOptions, ct);
    }

    public async Task CreateRealmAsync(
        string realmName, CancellationToken ct = default)
    {
        var realm = new { realm = realmName, enabled = true, registrationAllowed = false };
        var response = await _httpClient.PostAsJsonAsync("/admin/realms", realm, JsonOptions, ct);
        response.EnsureSuccessStatusCode();
    }

    public async Task<JsonElement[]?> GetClientsByClientIdAsync(
        string realmName, string clientId, CancellationToken ct = default)
    {
        var response = await _httpClient.GetAsync(
            $"/admin/realms/{realmName}/clients?clientId={Uri.EscapeDataString(clientId)}", ct);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<JsonElement[]>(JsonOptions, ct);
    }

    public async Task CreateClientAsync<T>(
        string realmName,
        T clientRepresentation,
        CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PostAsJsonAsync(
            $"/admin/realms/{realmName}/clients", clientRepresentation, JsonOptions, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public async Task UpdateClientAsync<T>(
        string realmName,
        string id,
        T clientRepresentation,
        CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PutAsJsonAsync(
            $"/admin/realms/{realmName}/clients/{id}", clientRepresentation, JsonOptions, cancellationToken);
        response.EnsureSuccessStatusCode();
    }
}
