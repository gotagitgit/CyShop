using Customers.Application.DTOs;

namespace Customers.Application.Interfaces;

public interface ICustomerAddressService
{
    Task<IReadOnlyList<CustomerAddressDto>> GetAddressesByExternalIdAsync(Guid externalId, CancellationToken ct = default);
    Task<CustomerAddressDto> CreateAsync(Guid externalId, CreateAddressDto dto, CancellationToken ct = default);
    Task<CustomerAddressDto> UpdateAsync(Guid externalId, Guid addressId, UpdateAddressDto dto, CancellationToken ct = default);
    Task<bool> DeleteAsync(Guid externalId, Guid addressId, CancellationToken ct = default);
}
