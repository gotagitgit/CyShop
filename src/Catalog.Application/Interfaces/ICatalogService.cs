using Catalog.Application.DTOs;

namespace Catalog.Application.Interfaces;

public interface ICatalogService
{
    Task<IReadOnlyList<CatalogItemDto>> GetAllItemsAsync(CancellationToken ct = default);
    Task<CatalogItemDto?> GetItemByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<CatalogItemDto>> GetItemsByTypeAsync(Guid typeId, CancellationToken ct = default);
    Task<IReadOnlyList<CatalogItemDto>> GetItemsByBrandAsync(Guid brandId, CancellationToken ct = default);
}
