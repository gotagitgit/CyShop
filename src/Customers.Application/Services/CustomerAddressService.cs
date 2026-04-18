using Customers.Application.DTOs;
using Customers.Application.Interfaces;
using Customers.Domain.Interfaces;

namespace Customers.Application.Services;

public class CustomerAddressService(
    ICustomerRepository customerRepository,
    ICustomerAddressRepository addressRepository) : ICustomerAddressService
{
    public async Task<IReadOnlyList<CustomerAddressDto>> GetAddressesByEmailAsync(string email, CancellationToken ct = default)
    {
        var customer = await customerRepository.GetByEmailAsync(email, ct);
        if (customer is null)
            return [];

        var addresses = await addressRepository.GetByCustomerIdAsync(customer.Id, ct);

        return addresses.Select(a => new CustomerAddressDto(
            a.Id,
            a.CustomerId,
            a.Label,
            a.Street,
            a.City,
            a.State,
            a.Country,
            a.ZipCode,
            a.IsDefault,
            a.CreatedDateTime)).ToList();
    }
}
