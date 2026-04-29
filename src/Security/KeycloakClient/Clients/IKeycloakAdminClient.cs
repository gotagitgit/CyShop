using System.Text.Json;
using Keycloak.AuthServices.Sdk.Admin.Models;

namespace KeycloakClient.Clients;

public interface IKeycloakAdminClient
{
    Task<RealmRepresentation?> GetRealmAsync(string realmName, CancellationToken ct = default);

    Task CreateRealmAsync(string realmName, CancellationToken ct = default);

    Task DeleteRealmAsync(string realmName, CancellationToken ct = default);

    Task<UserRepresentation[]> GetUsersAsync(
        string realmName, string? username = null, bool exact = false, CancellationToken ct = default);

    Task<UserRepresentation> GetUserAsync(
        string realmName, string userId, CancellationToken ct = default);

    Task CreateUserAsync(
        string realmName, UserRepresentation user, CancellationToken ct = default);

    Task UpdateUserAsync(
        string realmName, string userId, UserRepresentation user, CancellationToken ct = default);

    Task DeleteUserAsync(
        string realmName, string userId, CancellationToken ct = default);

    Task<JsonElement[]?> GetClientsByClientIdAsync(
        string realmName, string clientId, CancellationToken ct = default);

    Task CreateClientAsync<T>(
        string realmName, T clientRepresentation, CancellationToken ct = default);

    Task UpdateClientAsync<T>(
        string realmName, string id, T clientRepresentation, CancellationToken ct = default);

    Task<JsonElement[]> GetClientScopesAsync(
        string realmName, CancellationToken ct = default);

    Task CreateClientScopeAsync(
        string realmName, string scopeName, CancellationToken ct = default);

    Task<UserRepresentation> GetServiceAccountUserAsync(
        string realmName, string clientInternalId, CancellationToken ct = default);

    Task AddDefaultClientScopeAsync(
        string realmName, string clientInternalId, string scopeId, CancellationToken ct = default);
}
