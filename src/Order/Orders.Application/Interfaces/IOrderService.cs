using Orders.Application.DTOs;

namespace Orders.Application.Interfaces;

public interface IOrderService
{
    Task CreateOrderAsync(Guid customerId, CreateOrderDto dto, CancellationToken ct = default);
    Task<IReadOnlyList<OrderDto>> GetOrdersByCustomerAsync(Guid customerId, CancellationToken ct = default);
    Task<OrderDetailDto?> GetOrderByIdAsync(Guid orderId, Guid customerId, CancellationToken ct = default);
}
