using Customers.Domain.Entities;

namespace Customers.Domain.Interfaces;

public interface ICustomerAddressRepository
{
    Task<IReadOnlyList<CustomerAddress>> GetByCustomerIdAsync(Guid customerId, CancellationToken ct = default);
    Task<CustomerAddress?> GetByIdAsync(Guid addressId, CancellationToken ct = default);
    Task<CustomerAddress> AddAsync(CustomerAddress address, CancellationToken ct = default);
    Task<CustomerAddress> UpdateAsync(CustomerAddress address, CancellationToken ct = default);
    Task<bool> DeleteAsync(Guid addressId, CancellationToken ct = default);
    Task ClearDefaultAsync(Guid customerId, CancellationToken ct = default);
}
