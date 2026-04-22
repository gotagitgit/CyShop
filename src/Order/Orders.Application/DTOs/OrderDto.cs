namespace Orders.Application.DTOs;

public record OrderShippingAddressDto(
    string Street,
    string City,
    string State,
    string Country,
    string ZipCode);

public record OrderDto(
    Guid OrderId,
    DateTime OrderDate,
    string Status,
    string CustomerName,
    decimal TotalAmount,
    int ItemCount,
    OrderShippingAddressDto ShippingAddress);
