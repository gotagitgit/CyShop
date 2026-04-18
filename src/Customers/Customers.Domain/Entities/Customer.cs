namespace Customers.Domain.Entities;

public sealed record Customer(
    Guid Id,
    Guid ExternalId,
    string FirstName,
    string LastName,
    string Email,
    string ContactNumber);
