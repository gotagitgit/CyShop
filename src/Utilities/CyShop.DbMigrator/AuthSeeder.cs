using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Auth.Infrastructure.Services;
using Auth.Infrastructure.Dtos;
using Auth.Infrastructure.Extensions;
using Customers.Domain.Interfaces;
using Customers.Domain.Entities;
using CyShop.DbMigrator.Models;

namespace CyShop.DbMigrator;

public sealed class AuthSeeder(
    IConfiguration configuration,
    IIdentityProviderService identityProviderService,
    ICustomerRepository customerRepository,
    DevUser devUser,
    ILogger<AuthSeeder> logger)
{
    public async Task SeedAsync(CancellationToken cancellationToken)
    {
        var realmName = configuration["Keycloak:Realm"] ?? "cyshop";

        await identityProviderService.CreateRealmAsync(realmName);

        await identityProviderService.CreateClientAsync(realmName, "cyshop-web", cancellationToken);

        var clientSecret = configuration["Keycloak:ServiceClientSecret"] ?? "service-secret";
        var serviceClient = new CreateClientDto().CreateConfidential("cyshop-service", clientSecret);
        await identityProviderService.CreateClientAsync(realmName, serviceClient, cancellationToken);

        var apiScopes = new[] { "basket.api", "catalog.api", "customers.api" };
        foreach (var scope in apiScopes)
        {
            await identityProviderService.CreateClientScopeAsync(realmName, scope, cancellationToken);
        }
        await identityProviderService.AssignClientScopesAsync(realmName, "cyshop-service", apiScopes, cancellationToken);

        var users = await GetCustomersAsync(cancellationToken);

        var createUserDtos = ToDto(users);

        var createdUsers = new List<UserDto>();

        foreach (var user in createUserDtos)
        {
            await identityProviderService.CreateUserAsync(realmName, user, cancellationToken);

            var createdUser = await identityProviderService.FindUserAsync(realmName, user.UserName, cancellationToken);

            if (createdUser != null) 
                createdUsers.Add(createdUser);
        }

        await UpdateCustomerAsync([.. createdUsers], users, cancellationToken);

        devUser.Set(createdUsers[0].Id);
    }

    private async Task UpdateCustomerAsync(
        UserDto[] createdUsers,
        Customer[] users,
        CancellationToken cancellationToken)
    {
        var pairs = users.Join(createdUsers, c => c.FirstName, u => u.UserName, (c, u) => (Customer: c, User: u));

        foreach (var (customer, user) in pairs)
        {
            await customerRepository.UpdateExternalIdAsync(customer.Id, user.Id, cancellationToken);
        }
    }

    private async Task<Customer[]> GetCustomersAsync(CancellationToken cancellationToken) =>
        await customerRepository.GetAllAsync(cancellationToken);

    private static CreateUserDto[] ToDto(Customer[] customers) =>
        customers.Select(c => new CreateUserDto
        {
            UserName = c.FirstName,
            Password = c.FirstName,
            FirstName = c.FirstName,
            LastName = c.LastName,
            Email = c.Email
        }).ToArray();
}
