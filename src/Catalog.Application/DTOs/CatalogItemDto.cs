using Catalog.Domain.Entities;

namespace Catalog.Application.DTOs;

public record CatalogItemDto(
    Guid Id,
    CatalogType Type,
    CatalogBrand Brand,
    string Name,
    string Description,
    decimal Price,
    string ImagePath);
