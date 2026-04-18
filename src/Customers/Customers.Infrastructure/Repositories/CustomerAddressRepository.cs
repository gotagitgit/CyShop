using Customers.Domain.Entities;
using Customers.Domain.Interfaces;
using Customers.Infrastructure.Data;
using Customers.Infrastructure.Data.Mappers;
using Microsoft.EntityFrameworkCore;

namespace Customers.Infrastructure.Repositories;

public class CustomerAddressRepository(CustomersDbContext context) : ICustomerAddressRepository
{
    public async Task<IReadOnlyList<CustomerAddress>> GetByCustomerIdAsync(Guid customerId, CancellationToken ct = default)
    {
        var dtos = await context.CustomerAddresses
            .AsNoTracking()
            .Where(a => a.CustomerId == customerId)
            .ToListAsync(ct);

        return dtos.Select(CustomerMapper.ToDomain).ToList();
    }
}
