using System;
using System.Collections.Generic;
using System.Text;
using Keycloak.AuthServices.Sdk.Admin.Models;
using Keycloak.AuthServices.Sdk.Admin.Requests.Users;

namespace KeycloakClient.Clients;

public partial class KeycloakAdminClient
{
    public async Task<UserRepresentation[]> GetUsersAsync(
        string realmName, string? username = null, bool exact = false, CancellationToken ct = default)
    {
        var parameters = new GetUsersRequestParameters
        {
            Username = username,
            Exact = exact ? true : null
        };
        
        var users = await _sdk.GetUsersAsync(realmName, parameters, ct);

        return [.. users];
    }

    public async Task<UserRepresentation> GetUserAsync(
        string realmName, string userId, CancellationToken ct = default)
    {
        return await _sdk.GetUserAsync(realmName, userId, cancellationToken: ct);
    }

    public async Task CreateUserAsync(
        string realmName, UserRepresentation user, CancellationToken ct = default)
    {
        await _sdk.CreateUserAsync(realmName, user, ct);
    }

    public async Task UpdateUserAsync(
        string realmName, string userId, UserRepresentation user, CancellationToken ct = default)
    {
        await _sdk.UpdateUserAsync(realmName, userId, user, ct);
    }

    public async Task DeleteUserAsync(
        string realmName, string userId, CancellationToken ct = default)
    {
        await _sdk.DeleteUserAsync(realmName, userId, ct);
    }
}
