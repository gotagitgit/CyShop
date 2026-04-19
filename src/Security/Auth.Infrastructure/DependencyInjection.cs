using Auth.Infrastructure.Factories;
using Auth.Infrastructure.Services;
using KeycloakClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Auth.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddAuthInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddKeycloakAdminClient(configuration);

        services.AddScoped<IIdentityProviderService, IdentityProviderService>();
        services.AddScoped<IUserFactory, UserFactory>();
        return services;
    }
}
