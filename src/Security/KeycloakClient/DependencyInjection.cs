using Keycloak.AuthServices.Sdk;
using KeycloakClient.Clients;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace KeycloakClient;

public static class DependencyInjection
{
    public static IServiceCollection AddKeycloakAdminClient(
        this IServiceCollection services,
        IConfiguration configuration,
        string keycloakSectionName = "Keycloak")
    {
        services.AddTransient(sp =>
            new AdminTokenDelegatingHandler(configuration, keycloakSectionName));

        services.AddKeycloakAdminHttpClient(configuration, keycloakClientSectionName: keycloakSectionName)
            .AddHttpMessageHandler<AdminTokenDelegatingHandler>();

        services.AddHttpClient<IKeycloakAdminClient, KeycloakAdminClient>((sp, client) =>
        {
            var section = configuration.GetSection(keycloakSectionName);
            var baseUrl = section["auth-server-url"] ?? section["BaseUrl"] ?? "http://localhost:8080";
            client.BaseAddress = new Uri(baseUrl);
        })
        .AddHttpMessageHandler<AdminTokenDelegatingHandler>();

        return services;
    }
}
