namespace Customers.Application.DTOs;

public record CustomerAddressDto(
    Guid Id,
    Guid CustomerId,
    string Label,
    string Street,
    string City,
    string State,
    string Country,
    string ZipCode,
    bool IsDefault,
    DateTime CreatedDateTime);
