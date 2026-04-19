namespace Catalog.Application.DTOs;

public record UpdateCatalogItemDto(
    string Name,
    string Description,
    decimal Price,
    string BrandName,
    string TypeName,
    string ImagePath);
