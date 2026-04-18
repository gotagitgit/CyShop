namespace Customers.Application.DTOs;

public record CreateAddressDto(string Label, string Street, string City, string State, string Country, string ZipCode, bool IsDefault);
