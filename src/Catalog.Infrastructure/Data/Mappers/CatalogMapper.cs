using Catalog.Domain.Entities;
using Catalog.Infrastructure.Data.Dtos;

namespace Catalog.Infrastructure.Data.Mappers;

internal static class CatalogMapper
{
    public static CatalogItem ToDomain(CatalogItemDto dto) =>
        new(
            id: dto.Id,
            type: new CatalogType(dto.Type.Id, dto.Type.Name),
            brand: new CatalogBrand(dto.Brand.Id, dto.Brand.Name),
            name: dto.Name,
            description: dto.Description,
            price: dto.Price,
            imagePath: dto.ImagePath
        );

    public static CatalogItemDto ToDto(CatalogItem item) =>
        new()
        {
            Id = item.Id,
            Name = item.Name,
            Description = item.Description,
            Price = item.Price,
            ImagePath = item.ImagePath,
            TypeId = item.Type.id,
            BrandId = item.Brand.id,
        };

    public static CatalogBrandDto ToDto(CatalogBrand brand) =>
        new() { Id = brand.id, Name = brand.Name };

    public static CatalogTypeDto ToDto(CatalogType type) =>
        new() { Id = type.id, Name = type.Name };
}
