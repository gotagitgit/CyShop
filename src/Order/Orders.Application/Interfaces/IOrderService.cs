using Orders.Application.DTOs;

namespace Orders.Application.Interfaces;

public interface IOrderService
{
    Task CreateOrderAsync(CreateOrderDto dto, CancellationToken ct = default);
    Task<IReadOnlyList<OrderDto>> GetOrdersByCustomerAsync(CancellationToken ct = default);
    Task<OrderDetailDto?> GetOrderByIdAsync(Guid orderId, CancellationToken ct = default);
}
