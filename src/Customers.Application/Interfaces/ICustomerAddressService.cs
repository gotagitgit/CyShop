using Customers.Application.DTOs;

namespace Customers.Application.Interfaces;

public interface ICustomerAddressService
{
    Task<IReadOnlyList<CustomerAddressDto>> GetAddressesByEmailAsync(string email, CancellationToken ct = default);
}
