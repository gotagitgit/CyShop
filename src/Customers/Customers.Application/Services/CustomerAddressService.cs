using Customers.Application.DTOs;
using Customers.Application.Interfaces;
using Customers.Domain.Entities;
using Customers.Domain.Interfaces;

namespace Customers.Application.Services;

public class CustomerAddressService(
    ICustomerRepository customerRepository,
    ICustomerAddressRepository addressRepository) : ICustomerAddressService
{
    public async Task<IReadOnlyList<CustomerAddressDto>> GetAddressesByExternalIdAsync(Guid externalId, CancellationToken ct = default)
    {
        var customer = await customerRepository.GetByExternalIdAsync(externalId, ct);
        if (customer is null)
            return [];

        var addresses = await addressRepository.GetByCustomerIdAsync(customer.Id, ct);

        return addresses.Select(ToDto).ToList();
    }

    public async Task<CustomerAddressDto> CreateAsync(Guid externalId, CreateAddressDto dto, CancellationToken ct = default)
    {
        ValidateRequiredFields(dto.Label, dto.Street, dto.City, dto.State, dto.Country, dto.ZipCode);

        var customer = await customerRepository.GetByExternalIdAsync(externalId, ct)
            ?? throw new KeyNotFoundException("Customer not found");

        if (dto.IsDefault)
            await addressRepository.ClearDefaultAsync(customer.Id, ct);

        var address = new CustomerAddress(
            Guid.NewGuid(),
            customer.Id,
            dto.Label,
            dto.Street,
            dto.City,
            dto.State,
            dto.Country,
            dto.ZipCode,
            dto.IsDefault,
            DateTime.UtcNow);

        var created = await addressRepository.AddAsync(address, ct);
        return ToDto(created);
    }

    public async Task<CustomerAddressDto> UpdateAsync(Guid externalId, Guid addressId, UpdateAddressDto dto, CancellationToken ct = default)
    {
        ValidateRequiredFields(dto.Label, dto.Street, dto.City, dto.State, dto.Country, dto.ZipCode);

        var customer = await customerRepository.GetByExternalIdAsync(externalId, ct)
            ?? throw new KeyNotFoundException("Customer not found");

        var existing = await addressRepository.GetByIdAsync(addressId, ct);
        if (existing is null || existing.CustomerId != customer.Id)
            throw new KeyNotFoundException("Address not found");

        if (dto.IsDefault)
            await addressRepository.ClearDefaultAsync(customer.Id, ct);

        var updated = existing with
        {
            Label = dto.Label,
            Street = dto.Street,
            City = dto.City,
            State = dto.State,
            Country = dto.Country,
            ZipCode = dto.ZipCode,
            IsDefault = dto.IsDefault
        };

        var result = await addressRepository.UpdateAsync(updated, ct);
        return ToDto(result);
    }

    public async Task<bool> DeleteAsync(Guid externalId, Guid addressId, CancellationToken ct = default)
    {
        var customer = await customerRepository.GetByExternalIdAsync(externalId, ct)
            ?? throw new KeyNotFoundException("Customer not found");

        var existing = await addressRepository.GetByIdAsync(addressId, ct);
        if (existing is null || existing.CustomerId != customer.Id)
            throw new KeyNotFoundException("Address not found");

        return await addressRepository.DeleteAsync(addressId, ct);
    }

    private static CustomerAddressDto ToDto(CustomerAddress a) =>
        new(a.Id,
            a.CustomerId,
            a.Label,
            a.Street,
            a.City,
            a.State,
            a.Country,
            a.ZipCode,
            a.IsDefault,
            a.CreatedDateTime);

    private static void ValidateRequiredFields(string label, string street, string city, string state, string country, string zipCode)
    {
        var errors = new Dictionary<string, string[]>();

        if (string.IsNullOrWhiteSpace(label))
            errors["Label"] = ["Label is required"];
        if (string.IsNullOrWhiteSpace(street))
            errors["Street"] = ["Street is required"];
        if (string.IsNullOrWhiteSpace(city))
            errors["City"] = ["City is required"];
        if (string.IsNullOrWhiteSpace(state))
            errors["State"] = ["State is required"];
        if (string.IsNullOrWhiteSpace(country))
            errors["Country"] = ["Country is required"];
        if (string.IsNullOrWhiteSpace(zipCode))
            errors["ZipCode"] = ["ZipCode is required"];

        if (errors.Count > 0)
            throw new ArgumentException(
                string.Join("; ", errors.SelectMany(e => e.Value)));
    }
}
