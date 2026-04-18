using Customers.Domain.Entities;

namespace Customers.Domain.Interfaces;

public interface ICustomerAddressRepository
{
    Task<IReadOnlyList<CustomerAddress>> GetByCustomerIdAsync(Guid customerId, CancellationToken ct = default);
}
