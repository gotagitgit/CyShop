using Auth.Infrastructure.Dtos;
using Auth.Infrastructure.Extensions;
using Auth.Infrastructure.Factories;
using KeycloakClient.Clients;
using Microsoft.Extensions.Logging;

namespace Auth.Infrastructure.Services;

internal sealed class IdentityProviderService : IIdentityProviderService
{
    private readonly IKeycloakAdminClient _keycloakAdminClient;
    private readonly ILogger<IdentityProviderService> _logger;
    private readonly IUserFactory _userFactory;

    public IdentityProviderService(
        IKeycloakAdminClient keycloakAdminClient,
        ILogger<IdentityProviderService> logger,
        IUserFactory userFactory)
    {
        _keycloakAdminClient = keycloakAdminClient;
        _logger = logger;
        _userFactory = userFactory;
    }

    public async Task CreateRealmAsync(string realmName)
    {
        var existing = await _keycloakAdminClient.GetRealmAsync(realmName);

        if (existing is not null)
        {
            _logger.LogInformation("Realm '{Realm}' already exists.", realmName);
            return;
        }

        await _keycloakAdminClient.CreateRealmAsync(realmName);
        _logger.LogInformation("Realm '{Realm}' created.", realmName);
    }

    public async Task DeleteRealmAsync(string realmName)
    {
        await _keycloakAdminClient.DeleteRealmAsync(realmName);
        _logger.LogInformation("Realm '{Realm}' deleted.", realmName);
    }

    public async Task CreateClientAsync(string realmName, string clientId, CancellationToken cancellationToken)
    {
        var client = new CreateClientDto().CreateDefault(clientId);

        await CreateClientAsync(realmName, client, cancellationToken);
    }

    public async Task CreateClientAsync(string realmName, CreateClientDto client, CancellationToken cancellationToken)
    {
        var clientId = client.ClientId;

        var clients = await _keycloakAdminClient.GetClientsByClientIdAsync(realmName, clientId);

        if (clients is { Length: > 0 })
        {
            _logger.LogInformation("Client '{ClientId}' already exists in realm '{Realm}'.", clientId, realmName);

            return;
        }

        await _keycloakAdminClient.CreateClientAsync(realmName, client, cancellationToken);

        _logger.LogInformation("Client '{ClientId}' created in realm '{Realm}'.", clientId, realmName);
    }

    public async Task CreateUserAsync(string realmName, CreateUserDto user, CancellationToken cancellationToken)
    {
        var users = await _keycloakAdminClient.GetUsersAsync(realmName, user.UserName, exact: true);

        if (users is { Length: > 0 })
        {
            _logger.LogInformation("User '{Username}' already exists in realm '{Realm}'.", user.UserName, realmName);
            return;
        }

        var userRepresentation = user.ToUserRepresentation();

        await _keycloakAdminClient.CreateUserAsync(realmName, userRepresentation, cancellationToken);

        _logger.LogInformation("User '{Username}' created in realm '{Realm}'.", user.UserName, realmName);
    }

    public async Task<UserDto?> FindUserAsync(string realmName, string username, CancellationToken cancellationToken)
    {
        var user = await _keycloakAdminClient.GetUsersAsync(realmName, username, true, cancellationToken);

        return _userFactory.Create(user.First());
    }

    public async Task CreateClientScopeAsync(string realmName, string scopeName, CancellationToken cancellationToken)
    {
        var existingScopes = await _keycloakAdminClient.GetClientScopesAsync(realmName, cancellationToken);

        var alreadyExists = existingScopes.Any(s =>
            s.TryGetProperty("name", out var name) && name.GetString() == scopeName);

        if (alreadyExists)
        {
            _logger.LogInformation("Client scope '{ScopeName}' already exists in realm '{Realm}'.", scopeName, realmName);
            return;
        }

        await _keycloakAdminClient.CreateClientScopeAsync(realmName, scopeName, cancellationToken);
        _logger.LogInformation("Client scope '{ScopeName}' created in realm '{Realm}'.", scopeName, realmName);
    }

    public async Task AssignClientScopesAsync(string realmName, string clientId, string[] scopeNames, CancellationToken cancellationToken)
    {
        // Look up the client's internal ID
        var clients = await _keycloakAdminClient.GetClientsByClientIdAsync(realmName, clientId, cancellationToken);

        if (clients is not { Length: > 0 })
        {
            _logger.LogWarning("Client '{ClientId}' not found in realm '{Realm}'. Cannot assign scopes.", clientId, realmName);
            return;
        }

        var clientInternalId = clients[0].GetProperty("id").GetString()!;

        // Get all client scopes to resolve scope names to IDs
        var allScopes = await _keycloakAdminClient.GetClientScopesAsync(realmName, cancellationToken);

        foreach (var scopeName in scopeNames)
        {
            var scope = allScopes.FirstOrDefault(s =>
                s.TryGetProperty("name", out var name) && name.GetString() == scopeName);

            if (scope.ValueKind == System.Text.Json.JsonValueKind.Undefined)
            {
                _logger.LogWarning("Client scope '{ScopeName}' not found in realm '{Realm}'. Skipping assignment.", scopeName, realmName);
                continue;
            }

            var scopeId = scope.GetProperty("id").GetString()!;

            await _keycloakAdminClient.AddDefaultClientScopeAsync(realmName, clientInternalId, scopeId, cancellationToken);
            _logger.LogInformation("Assigned scope '{ScopeName}' to client '{ClientId}' in realm '{Realm}'.", scopeName, clientId, realmName);
        }
    }
}
