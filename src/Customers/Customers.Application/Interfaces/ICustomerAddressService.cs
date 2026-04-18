using Customers.Application.DTOs;

namespace Customers.Application.Interfaces;

public interface ICustomerAddressService
{
    Task<IReadOnlyList<CustomerAddressDto>> GetAddressesByExternalIdAsync(Guid externalId, CancellationToken ct = default);
}
