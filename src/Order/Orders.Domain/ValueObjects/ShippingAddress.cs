namespace Orders.Domain.ValueObjects;

public sealed record ShippingAddress(
    string Street,
    string City,
    string State,
    string Country,
    string ZipCode);
