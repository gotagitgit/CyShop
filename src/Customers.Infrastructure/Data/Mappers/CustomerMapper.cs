using Customers.Domain.Entities;
using Customers.Infrastructure.Data.Dtos;

namespace Customers.Infrastructure.Data.Mappers;

internal static class CustomerMapper
{
    public static Customer ToDomain(CustomerDto dto) =>
        new(
            Id: dto.Id,
            FirstName: dto.FirstName,
            LastName: dto.LastName,
            Email: dto.Email,
            ContactNumber: dto.ContactNumber
        );

    public static CustomerAddress ToDomain(CustomerAddressDto dto) =>
        new(
            Id: dto.Id,
            CustomerId: dto.CustomerId,
            Label: dto.Label,
            Street: dto.Street,
            City: dto.City,
            State: dto.State,
            Country: dto.Country,
            ZipCode: dto.ZipCode,
            IsDefault: dto.IsDefault,
            CreatedDateTime: dto.CreatedDateTime
        );

    public static CustomerDto ToDto(Customer entity) =>
        new()
        {
            Id = entity.Id,
            FirstName = entity.FirstName,
            LastName = entity.LastName,
            Email = entity.Email,
            ContactNumber = entity.ContactNumber
        };

    public static CustomerAddressDto ToDto(CustomerAddress entity) =>
        new()
        {
            Id = entity.Id,
            CustomerId = entity.CustomerId,
            Label = entity.Label,
            Street = entity.Street,
            City = entity.City,
            State = entity.State,
            Country = entity.Country,
            ZipCode = entity.ZipCode,
            IsDefault = entity.IsDefault,
            CreatedDateTime = entity.CreatedDateTime
        };
}
