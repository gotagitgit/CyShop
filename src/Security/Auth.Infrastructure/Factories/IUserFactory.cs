using Auth.Infrastructure.Dtos;
using Keycloak.AuthServices.Sdk.Admin.Models;

namespace Auth.Infrastructure.Factories;

internal interface IUserFactory
{
    UserDto? Create(UserRepresentation userRepresentation);
}