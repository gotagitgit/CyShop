using Customers.Domain.Entities;
using Customers.Domain.Interfaces;
using Customers.Infrastructure.Data;
using Customers.Infrastructure.Data.Mappers;
using Microsoft.EntityFrameworkCore;

namespace Customers.Infrastructure.Repositories;

public class CustomerRepository(CustomersDbContext context) : ICustomerRepository
{
    public async Task<Customer[]> GetAllAsync(CancellationToken ct = default)
    {
        var dtos = await context.Customers
            .AsNoTracking()
            .ToListAsync(ct);
        return dtos.Select(CustomerMapper.ToDomain).ToArray();
    }

    public async Task<Customer?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var dto = await context.Customers
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id, ct);

        return dto is null ? null : CustomerMapper.ToDomain(dto);
    }

    public async Task<Customer?> GetByExternalIdAsync(Guid externalId, CancellationToken ct = default)
    {
        var dto = await context.Customers
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.ExternalId == externalId, ct);

        return dto is null ? null : CustomerMapper.ToDomain(dto);
    }

    public async Task<Customer?> GetByEmailAsync(string email, CancellationToken ct = default)
    {
        var dto = await context.Customers
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Email == email, ct);

        return dto is null ? null : CustomerMapper.ToDomain(dto);
    }

    public async Task<Customer> AddAsync(Customer customer, CancellationToken ct = default)
    {
        var dto = CustomerMapper.ToDto(customer);
        context.Customers.Add(dto);
        await context.SaveChangesAsync(ct);
        return CustomerMapper.ToDomain(dto);
    }

    public async Task<Customer> UpdateAsync(Customer customer, CancellationToken ct = default)
    {
        var dto = await context.Customers
            .FirstOrDefaultAsync(c => c.Id == customer.Id, ct)
            ?? throw new InvalidOperationException($"Customer with id {customer.Id} not found.");

        dto.FirstName = customer.FirstName;
        dto.LastName = customer.LastName;
        dto.Email = customer.Email;
        dto.ContactNumber = customer.ContactNumber;

        await context.SaveChangesAsync(ct);
        return CustomerMapper.ToDomain(dto);
    }

    public async Task<Customer> UpdateExternalIdAsync(Guid id, Guid externalId, CancellationToken ct = default)
    {
        var dto = await context.Customers
            .FirstOrDefaultAsync(c => c.Id == id, ct)
            ?? throw new InvalidOperationException($"Customer with id {id} not found.");

        dto.ExternalId = externalId;

        await context.SaveChangesAsync(ct);
        return CustomerMapper.ToDomain(dto);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var dto = await context.Customers
            .FirstOrDefaultAsync(c => c.Id == id, ct);

        if (dto is null)
            return false;

        context.Customers.Remove(dto);
        await context.SaveChangesAsync(ct);
        return true;
    }
}
