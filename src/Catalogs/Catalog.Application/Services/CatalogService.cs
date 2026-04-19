using Catalog.Application.DTOs;
using Catalog.Application.Interfaces;
using Catalog.Domain.Entities;
using Catalog.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using Storage.Infrastructure.Services;

namespace Catalog.Application.Services;

public class CatalogService(
    ICatalogRepository repository,
    IStorageService storageService,
    IConfiguration configuration) : ICatalogService
{
    private readonly string _bucketName = configuration["Storage:BucketName"]
        ?? "catalog-images";

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

    public async Task<Stream?> GetItemImageAsync(Guid id, CancellationToken ct = default)
    {
        var item = await repository.GetByIdAsync(id, ct);
        if (item is null) return null;

        var exists = await storageService.ExistsAsync(_bucketName, item.ImagePath, ct);
        if (!exists) return null;

        return await storageService.DownloadAsync(_bucketName, item.ImagePath, ct);
    }

    private static IReadOnlyList<CatalogItemDto> MapToDto(IReadOnlyList<CatalogItem> items) =>
        items.Select(MapToDto).ToList();

    private static CatalogItemDto MapToDto(CatalogItem item) =>
        new(item.Id, item.Type, item.Brand, item.Name, item.Description, item.Price, item.ImagePath);
}
