namespace Catalog.Application.DTOs;

public record CreateCatalogItemDto(
    string Name,
    string Description,
    decimal Price,
    string BrandName,
    string TypeName,
    string ImagePath);
