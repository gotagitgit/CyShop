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

    public async Task<CustomerAddress?> GetByIdAsync(Guid addressId, CancellationToken ct = default)
    {
        var dto = await context.CustomerAddresses
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == addressId, ct);

        return dto is null ? null : CustomerMapper.ToDomain(dto);
    }

    public async Task<CustomerAddress> AddAsync(CustomerAddress address, CancellationToken ct = default)
    {
        var dto = CustomerMapper.ToDto(address);
        context.CustomerAddresses.Add(dto);
        await context.SaveChangesAsync(ct);
        return CustomerMapper.ToDomain(dto);
    }

    public async Task<CustomerAddress> UpdateAsync(CustomerAddress address, CancellationToken ct = default)
    {
        var dto = await context.CustomerAddresses
            .FirstOrDefaultAsync(a => a.Id == address.Id, ct)
            ?? throw new InvalidOperationException($"Address with id {address.Id} not found.");

        dto.Label = address.Label;
        dto.Street = address.Street;
        dto.City = address.City;
        dto.State = address.State;
        dto.Country = address.Country;
        dto.ZipCode = address.ZipCode;
        dto.IsDefault = address.IsDefault;

        await context.SaveChangesAsync(ct);
        return CustomerMapper.ToDomain(dto);
    }

    public async Task<bool> DeleteAsync(Guid addressId, CancellationToken ct = default)
    {
        var dto = await context.CustomerAddresses
            .FirstOrDefaultAsync(a => a.Id == addressId, ct);

        if (dto is null)
            return false;

        context.CustomerAddresses.Remove(dto);
        await context.SaveChangesAsync(ct);
        return true;
    }

    public async Task ClearDefaultAsync(Guid customerId, CancellationToken ct = default)
    {
        var defaults = await context.CustomerAddresses
            .Where(a => a.CustomerId == customerId && a.IsDefault)
            .ToListAsync(ct);

        foreach (var dto in defaults)
        {
            dto.IsDefault = false;
        }

        await context.SaveChangesAsync(ct);
    }
}
