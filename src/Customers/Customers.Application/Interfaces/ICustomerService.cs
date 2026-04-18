using Customers.Application.DTOs;

namespace Customers.Application.Interfaces;

public interface ICustomerService
{
    Task<CustomerDto?> GetByExternalIdAsync(Guid externalId, CancellationToken ct = default);
    Task<CustomerDto> CreateAsync(Guid externalId, CreateCustomerDto dto, CancellationToken ct = default);
    Task<CustomerDto> UpdateAsync(Guid externalId, UpdateCustomerDto dto, CancellationToken ct = default);
    Task<bool> DeleteAsync(Guid externalId, CancellationToken ct = default);
}
