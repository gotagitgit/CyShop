using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CyShop.DbMigrator;

public class KeycloakSeeder(IConfiguration configuration, ILogger<KeycloakSeeder> logger)
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public async Task SeedAsync()
    {
        var baseUrl = configuration["Keycloak:BaseUrl"] ?? "http://localhost:8080";
        var adminUser = configuration["Keycloak:AdminUser"] ?? "admin";
        var adminPassword = configuration["Keycloak:AdminPassword"] ?? "admin";
        var realmName = configuration["Keycloak:Realm"] ?? "cyshop";

        using var http = new HttpClient { BaseAddress = new Uri(baseUrl) };

        logger.LogInformation("[Keycloak] Authenticating as admin...");
        var token = await GetAdminTokenAsync(http, adminUser, adminPassword);
        http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        logger.LogInformation("[Keycloak] Ensuring realm '{Realm}' exists...", realmName);
        await EnsureRealmAsync(http, realmName);

        logger.LogInformation("[Keycloak] Ensuring client 'basket-api' exists...");
        await EnsureClientAsync(http, realmName);

        logger.LogInformation("[Keycloak] Seeding test users...");
        await EnsureUserAsync(http, realmName, "user", "Pass123$", "user@email.com", "Test", "User");
        await EnsureUserAsync(http, realmName, "admin", "Pass123$", "admin@email.com", "Test", "Admin");

        logger.LogInformation("[Keycloak] Seeding complete.");
    }

    private static async Task<string> GetAdminTokenAsync(HttpClient http, string user, string password)
    {
        var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["grant_type"] = "password",
            ["client_id"] = "admin-cli",
            ["username"] = user,
            ["password"] = password
        });

        var response = await http.PostAsync("/realms/master/protocol/openid-connect/token", content);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        return json.GetProperty("access_token").GetString()!;
    }

    private async Task EnsureRealmAsync(HttpClient http, string realmName)
    {
        var response = await http.GetAsync($"/admin/realms/{realmName}");

        if (response.IsSuccessStatusCode)
        {
            logger.LogInformation("[Keycloak] Realm '{Realm}' already exists.", realmName);
            return;
        }

        var realm = new
        {
            realm = realmName,
            enabled = true,
            registrationAllowed = false
        };

        var createResponse = await http.PostAsJsonAsync("/admin/realms", realm, JsonOptions);
        createResponse.EnsureSuccessStatusCode();
        logger.LogInformation("[Keycloak] Realm '{Realm}' created.", realmName);
    }

    private async Task EnsureClientAsync(HttpClient http, string realmName)
    {
        var searchResponse = await http.GetAsync($"/admin/realms/{realmName}/clients?clientId=basket-api");
        searchResponse.EnsureSuccessStatusCode();

        var clients = await searchResponse.Content.ReadFromJsonAsync<JsonElement[]>(JsonOptions);
        if (clients is { Length: > 0 })
        {
            logger.LogInformation("[Keycloak] Client 'basket-api' already exists in realm '{Realm}'.", realmName);
            return;
        }

        var client = new
        {
            clientId = "basket-api",
            enabled = true,
            publicClient = true,
            directAccessGrantsEnabled = true,
            standardFlowEnabled = false
        };

        var createResponse = await http.PostAsJsonAsync($"/admin/realms/{realmName}/clients", client, JsonOptions);
        createResponse.EnsureSuccessStatusCode();
        logger.LogInformation("[Keycloak] Client 'basket-api' created in realm '{Realm}'.", realmName);
    }

    private async Task EnsureUserAsync(
        HttpClient http,
        string realmName,
        string username,
        string password,
        string email,
        string firstName,
        string lastName)
    {
        var user = new
        {
            username,
            email,
            firstName,
            lastName,
            enabled = true,
            emailVerified = true,
            requiredActions = Array.Empty<string>(),
            credentials = new[]
            {
                new
                {
                    type = "password",
                    value = password,
                    temporary = false
                }
            }
        };

        var createResponse = await http.PostAsJsonAsync($"/admin/realms/{realmName}/users", user, JsonOptions);
        createResponse.EnsureSuccessStatusCode();
        logger.LogInformation("[Keycloak] User '{Username}' created in realm '{Realm}'.", username, realmName);
    }
}
