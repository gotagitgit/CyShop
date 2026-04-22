using Microsoft.EntityFrameworkCore;
using Orders.Domain.Entities;
using Orders.Domain.Interfaces;
using Orders.Infrastructure.Data;
using Orders.Infrastructure.Data.Mappers;

namespace Orders.Infrastructure.Repositories;

public class OrderRepository(OrdersDbContext context) : IOrderRepository
{
    public async Task AddAsync(Order order, CancellationToken ct = default)
    {
        var entity = OrderMapper.ToEntity(order);
        context.Orders.Add(entity);
        await context.SaveChangesAsync(ct);
    }

    public async Task<Order?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await context.Orders
            .AsNoTracking()
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == id, ct);

        return entity is null ? null : OrderMapper.ToDomain(entity);
    }

    public async Task<IReadOnlyList<Order>> GetByCustomerIdAsync(Guid customerId, CancellationToken ct = default)
    {
        var entities = await context.Orders
            .AsNoTracking()
            .Include(o => o.Items)
            .Where(o => o.CustomerId == customerId)
            .ToListAsync(ct);

        return entities.Select(OrderMapper.ToDomain).ToList();
    }
}
