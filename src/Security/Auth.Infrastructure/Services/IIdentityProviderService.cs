using Auth.Infrastructure.Dtos;

namespace Auth.Infrastructure.Services;

public interface IIdentityProviderService
{
    Task CreateClientAsync(string realmName, CreateClientDto client, CancellationToken cancellationToken);
    Task CreateClientAsync(string realmName, string clientId, CancellationToken cancellationToken);
    Task CreateRealmAsync(string realmName);
    Task DeleteRealmAsync(string realmName);
    Task CreateUserAsync(string realmName, CreateUserDto user, CancellationToken cancellationToken);
    Task<UserDto?> FindUserAsync(string realmName, string username, CancellationToken cancellationToken);
    Task CreateClientScopeAsync(string realmName, string scopeName, CancellationToken cancellationToken);
    Task AssignClientScopesAsync(string realmName, string clientId, string[] scopeNames, CancellationToken cancellationToken);
}
