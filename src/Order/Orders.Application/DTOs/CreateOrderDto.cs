namespace Orders.Application.DTOs;

public record CreateOrderItemDto(
    Guid ProductId,
    string ProductName,
    decimal UnitPrice,
    int Quantity);

public record CreateOrderShippingAddressDto(
    string Street,
    string City,
    string State,
    string Country,
    string ZipCode);

public record CreateOrderDto(
    string CustomerName,
    IReadOnlyList<CreateOrderItemDto> Items,
    CreateOrderShippingAddressDto ShippingAddress);
