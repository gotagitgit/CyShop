using Customers.Domain.Entities;
using Customers.Domain.Interfaces;
using Customers.Infrastructure.Data;
using Customers.Infrastructure.Data.Mappers;
using Microsoft.EntityFrameworkCore;

namespace Customers.Infrastructure.Repositories;

public class CustomerRepository(CustomersDbContext context) : ICustomerRepository
{
    public async Task<Customer?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var dto = await context.Customers
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id, ct);

        return dto is null ? null : CustomerMapper.ToDomain(dto);
    }

    public async Task<Customer?> GetByEmailAsync(string email, CancellationToken ct = default)
    {
        var dto = await context.Customers
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Email == email, ct);

        return dto is null ? null : CustomerMapper.ToDomain(dto);
    }
}
