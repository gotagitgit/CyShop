namespace Customers.Application.DTOs;

public record CustomerDto(
    Guid Id,
    Guid ExternalId,
    string FirstName,
    string LastName,
    string Email,
    string ContactNumber);
