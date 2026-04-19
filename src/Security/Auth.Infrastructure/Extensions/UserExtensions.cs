using Auth.Infrastructure.Dtos;
using Keycloak.AuthServices.Sdk.Admin.Models;

namespace Auth.Infrastructure.Extensions;

public static class UserExtensions
{
    extension(CreateUserDto dto)
    {
        public UserRepresentation ToUserRepresentation()
        {
            return  new UserRepresentation
            {
                Username = dto.UserName,
                Email = dto.Email,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Enabled = true,
                EmailVerified = true,
                RequiredActions = [],
                Credentials =
            [
                new CredentialRepresentation
                {
                    Type = "password",
                    Value = dto.Password,
                    Temporary = false
                }
            ]
            };
        }
    }
}
