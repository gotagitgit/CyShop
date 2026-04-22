namespace Orders.API.Models;

public record CreateOrderItemRequest(
    Guid ProductId,
    string ProductName,
    decimal UnitPrice,
    int Quantity);

public record CreateOrderShippingAddressRequest(
    string Street,
    string City,
    string State,
    string Country,
    string ZipCode);

public record CreateOrderRequest(
    string CustomerName,
    IReadOnlyList<CreateOrderItemRequest> Items,
    CreateOrderShippingAddressRequest ShippingAddress);
