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
}
