namespace Customers.Application.DTOs;

public record UpdateAddressDto(string Label, string Street, string City, string State, string Country, string ZipCode, bool IsDefault);
