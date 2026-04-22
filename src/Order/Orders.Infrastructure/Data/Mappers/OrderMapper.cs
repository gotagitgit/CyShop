using Orders.Domain.Entities;
using Orders.Domain.Enums;
using Orders.Domain.ValueObjects;
using Orders.Infrastructure.Data.Entities;

namespace Orders.Infrastructure.Data.Mappers;

internal static class OrderMapper
{
    public static Order ToDomain(OrderEntity entity) =>
        new()
        {
            Id = entity.Id,
            CustomerId = entity.CustomerId,
            CustomerName = entity.CustomerName,
            OrderDate = entity.OrderDate,
            Status = Enum.Parse<OrderStatus>(entity.Status),
            ShippingAddress = new ShippingAddress(
                entity.Street,
                entity.City,
                entity.State,
                entity.Country,
                entity.ZipCode),
            Items = entity.Items.Select(ToDomain).ToList()
        };

    public static OrderItem ToDomain(OrderItemEntity entity) =>
        new()
        {
            Id = entity.Id,
            ProductId = entity.ProductId,
            ProductName = entity.ProductName,
            UnitPrice = entity.UnitPrice,
            Quantity = entity.Quantity
        };

    public static OrderEntity ToEntity(Order order) =>
        new()
        {
            Id = order.Id,
            CustomerId = order.CustomerId,
            CustomerName = order.CustomerName,
            OrderDate = order.OrderDate,
            Status = order.Status.ToString(),
            Street = order.ShippingAddress.Street,
            City = order.ShippingAddress.City,
            State = order.ShippingAddress.State,
            Country = order.ShippingAddress.Country,
            ZipCode = order.ShippingAddress.ZipCode,
            Items = order.Items.Select(i => ToEntity(i, order.Id)).ToList()
        };

    public static OrderItemEntity ToEntity(OrderItem item, Guid orderId) =>
        new()
        {
            Id = item.Id,
            OrderId = orderId,
            ProductId = item.ProductId,
            ProductName = item.ProductName,
            UnitPrice = item.UnitPrice,
            Quantity = item.Quantity
        };
}
