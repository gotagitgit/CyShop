using Catalog.Domain.Entities;

namespace Catalog.Domain.Interfaces;

public interface ICatalogRepository
{
    Task<IReadOnlyList<CatalogItem>> GetAllAsync(CancellationToken ct = default);
    Task<CatalogItem?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<CatalogItem>> GetByTypeAsync(Guid typeId, CancellationToken ct = default);
    Task<IReadOnlyList<CatalogItem>> GetByBrandAsync(Guid brandId, CancellationToken ct = default);
    Task<bool> AnyAsync(CancellationToken ct = default);
    Task AddRangeAsync(IEnumerable<CatalogItem> items, CancellationToken ct = default);
    Task<CatalogItem> AddAsync(CatalogItem item, CancellationToken ct = default);
    Task UpdateAsync(CatalogItem item, CancellationToken ct = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken ct = default);
    Task<CatalogBrand> FindOrCreateBrandAsync(string name, CancellationToken ct = default);
    Task<CatalogType> FindOrCreateTypeAsync(string name, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
