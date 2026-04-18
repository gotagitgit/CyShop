namespace Customers.Domain.Entities;

public sealed record Customer(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    string ContactNumber);
