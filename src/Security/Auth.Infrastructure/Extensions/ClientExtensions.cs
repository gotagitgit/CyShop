using Auth.Infrastructure.Dtos;

namespace Auth.Infrastructure.Extensions;

public static class ClientExtensions
{
    extension(CreateClientDto dto)
    {
        public CreateClientDto CreateDefault(string clientId)
        {
            return new CreateClientDto
            {
                ClientId = clientId,
                Enabled = true,
                PublicClient = true,
                DirectAccessGrantsEnabled = true,
                StandardFlowEnabled = true,
                RedirectUris =  ["*"],
                WebOrigins = ["*"],
                Attributes =
                {
                    ["post.logout.redirect.uris"] = "+"
                }
            };
        }

        public CreateClientDto CreateConfidential(string clientId, string clientSecret)
        {
            return new CreateClientDto
            {
                ClientId = clientId,
                Enabled = true,
                PublicClient = false,
                ServiceAccountsEnabled = true,
                StandardFlowEnabled = false,
                DirectAccessGrantsEnabled = false,
                ClientSecret = clientSecret
            };
        }
    }
}
