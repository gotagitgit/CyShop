namespace Orders.Application.DTOs;

public record OrderItemDto(
    Guid Id,
    Guid ProductId,
    string ProductName,
    decimal UnitPrice,
    int Quantity);

public record OrderDetailDto(
    Guid OrderId,
    DateTime OrderDate,
    string Status,
    string CustomerName,
    decimal TotalAmount,
    IReadOnlyList<OrderItemDto> Items,
    OrderShippingAddressDto ShippingAddress);
