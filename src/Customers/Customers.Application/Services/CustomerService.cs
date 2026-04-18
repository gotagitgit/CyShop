using Customers.Application.DTOs;
using Customers.Application.Interfaces;
using Customers.Domain.Entities;
using Customers.Domain.Interfaces;

namespace Customers.Application.Services;

public class CustomerService(ICustomerRepository customerRepository) : ICustomerService
{
    public async Task<CustomerDto?> GetByExternalIdAsync(Guid externalId, CancellationToken ct = default)
    {
        var customer = await customerRepository.GetByExternalIdAsync(externalId, ct);
        return customer is null ? null : ToDto(customer);
    }

    public async Task<CustomerDto> CreateAsync(Guid externalId, CreateCustomerDto dto, CancellationToken ct = default)
    {
        ValidateRequiredFields(dto.FirstName, dto.LastName, dto.Email, dto.ContactNumber);

        var existing = await customerRepository.GetByExternalIdAsync(externalId, ct);
        if (existing is not null)
            throw new InvalidOperationException("Customer already exists");

        var customer = new Customer(
            Guid.NewGuid(),
            externalId,
            dto.FirstName,
            dto.LastName,
            dto.Email,
            dto.ContactNumber);

        var created = await customerRepository.AddAsync(customer, ct);
        return ToDto(created);
    }

    public async Task<CustomerDto> UpdateAsync(Guid externalId, UpdateCustomerDto dto, CancellationToken ct = default)
    {
        ValidateRequiredFields(dto.FirstName, dto.LastName, dto.Email, dto.ContactNumber);

        var existing = await customerRepository.GetByExternalIdAsync(externalId, ct);
        if (existing is null)
            throw new KeyNotFoundException("Customer not found");

        var updated = existing with
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Email = dto.Email,
            ContactNumber = dto.ContactNumber
        };

        var result = await customerRepository.UpdateAsync(updated, ct);
        return ToDto(result);
    }

    public async Task<bool> DeleteAsync(Guid externalId, CancellationToken ct = default)
    {
        var existing = await customerRepository.GetByExternalIdAsync(externalId, ct);
        if (existing is null)
            return false;

        return await customerRepository.DeleteAsync(existing.Id, ct);
    }

    private static CustomerDto ToDto(Customer customer) =>
        new(customer.Id,
            customer.ExternalId,
            customer.FirstName,
            customer.LastName,
            customer.Email,
            customer.ContactNumber);

    private static void ValidateRequiredFields(string firstName, string lastName, string email, string contactNumber)
    {
        var errors = new Dictionary<string, string[]>();

        if (string.IsNullOrWhiteSpace(firstName))
            errors["FirstName"] = ["FirstName is required"];
        if (string.IsNullOrWhiteSpace(lastName))
            errors["LastName"] = ["LastName is required"];
        if (string.IsNullOrWhiteSpace(email))
            errors["Email"] = ["Email is required"];
        if (string.IsNullOrWhiteSpace(contactNumber))
            errors["ContactNumber"] = ["ContactNumber is required"];

        if (errors.Count > 0)
            throw new ArgumentException(
                string.Join("; ", errors.SelectMany(e => e.Value)));
    }
}
