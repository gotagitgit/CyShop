using Catalog.Application.DTOs;
using Catalog.Application.Interfaces;
using Catalog.Domain.Entities;
using Catalog.Domain.Interfaces;

namespace Catalog.Application.Services;

public class CatalogService(ICatalogRepository repository) : ICatalogService
{
    public async Task<IReadOnlyList<CatalogItemDto>> GetAllItemsAsync(CancellationToken ct = default)
    {
        var items = await repository.GetAllAsync(ct);
        return MapToDto(items);
    }

    public async Task<CatalogItemDto?> GetItemByIdAsync(Guid id, CancellationToken ct = default)
    {
        var item = await repository.GetByIdAsync(id, ct);
        return item is null ? null : MapToDto(item);
    }

    public async Task<IReadOnlyList<CatalogItemDto>> GetItemsByTypeAsync(Guid typeId, CancellationToken ct = default)
    {
        var items = await repository.GetByTypeAsync(typeId, ct);
        return MapToDto(items);
    }

    public async Task<IReadOnlyList<CatalogItemDto>> GetItemsByBrandAsync(Guid brandId, CancellationToken ct = default)
    {
        var items = await repository.GetByBrandAsync(brandId, ct);
        return MapToDto(items);
    }

    private static IReadOnlyList<CatalogItemDto> MapToDto(IReadOnlyList<CatalogItem> items) =>
        items.Select(MapToDto).ToList();

    private static CatalogItemDto MapToDto(CatalogItem item) =>
        new(item.Id, item.Type, item.Brand, item.Name, item.Description, item.Price, item.ImagePath);
}
