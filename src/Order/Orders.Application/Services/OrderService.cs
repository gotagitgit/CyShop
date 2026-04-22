using Orders.Application.DTOs;
using Orders.Application.Interfaces;
using Orders.Domain.Entities;
using Orders.Domain.Enums;
using Orders.Domain.Interfaces;
using Orders.Domain.ValueObjects;

namespace Orders.Application.Services;

public class OrderService(IOrderRepository repository) : IOrderService
{
    public async Task CreateOrderAsync(Guid customerId, CreateOrderDto dto, CancellationToken ct = default)
    {
        var order = new Order
        {
            Id = Guid.NewGuid(),
            CustomerId = customerId,
            CustomerName = dto.CustomerName,
            OrderDate = DateTime.UtcNow,
            Status = OrderStatus.Submitted,
            Items = dto.Items.Select(i => new OrderItem
            {
                Id = Guid.NewGuid(),
                ProductId = i.ProductId,
                ProductName = i.ProductName,
                UnitPrice = i.UnitPrice,
                Quantity = i.Quantity
            }).ToList(),
            ShippingAddress = new ShippingAddress(
                dto.ShippingAddress.Street,
                dto.ShippingAddress.City,
                dto.ShippingAddress.State,
                dto.ShippingAddress.Country,
                dto.ShippingAddress.ZipCode)
        };

        await repository.AddAsync(order, ct);
    }

    public async Task<IReadOnlyList<OrderDto>> GetOrdersByCustomerAsync(Guid customerId, CancellationToken ct = default)
    {
        var orders = await repository.GetByCustomerIdAsync(customerId, ct);
        return orders.Select(MapToOrderDto).ToList();
    }

    public async Task<OrderDetailDto?> GetOrderByIdAsync(Guid orderId, Guid customerId, CancellationToken ct = default)
    {
        var order = await repository.GetByIdAsync(orderId, ct);

        if (order is null || order.CustomerId != customerId)
            return null;

        return MapToOrderDetailDto(order);
    }

    private static OrderDto MapToOrderDto(Order order) =>
        new(
            OrderId: order.Id,
            OrderDate: order.OrderDate,
            Status: order.Status.ToString(),
            CustomerName: order.CustomerName,
            TotalAmount: order.Items.Sum(i => i.UnitPrice * i.Quantity),
            ItemCount: order.Items.Count,
            ShippingAddress: MapToShippingAddressDto(order.ShippingAddress));

    private static OrderDetailDto MapToOrderDetailDto(Order order) =>
        new(
            OrderId: order.Id,
            OrderDate: order.OrderDate,
            Status: order.Status.ToString(),
            CustomerName: order.CustomerName,
            TotalAmount: order.Items.Sum(i => i.UnitPrice * i.Quantity),
            Items: order.Items.Select(i => new OrderItemDto(
                i.Id, i.ProductId, i.ProductName, i.UnitPrice, i.Quantity)).ToList(),
            ShippingAddress: MapToShippingAddressDto(order.ShippingAddress));

    private static OrderShippingAddressDto MapToShippingAddressDto(ShippingAddress addr) =>
        new(addr.Street, addr.City, addr.State, addr.Country, addr.ZipCode);
}
