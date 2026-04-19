using System.ComponentModel.DataAnnotations;
using Auth.Infrastructure.Dtos;
using Keycloak.AuthServices.Sdk.Admin.Models;

namespace Auth.Infrastructure.Factories;

internal sealed class UserFactory : IUserFactory
{
    public UserDto? Create(UserRepresentation userRepresentation)
    {
        if (userRepresentation is null)
            return null;

        if (!Guid.TryParse(userRepresentation.Id, out var userId))
            throw new ValidationException($"User Id {userRepresentation.Id} is not a valid Guid.");

        return new UserDto
        {
            Id = userId,
            UserName = userRepresentation?.Username ?? string.Empty,
            Email = userRepresentation?.Email ?? string.Empty
        };
    }
}
