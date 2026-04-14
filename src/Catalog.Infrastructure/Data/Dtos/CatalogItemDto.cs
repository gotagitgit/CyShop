namespace Catalog.Infrastructure.Data.Dtos;

public class CatalogItemDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string ImagePath { get; set; } = string.Empty;

    public Guid TypeId { get; set; }
    public CatalogTypeDto Type { get; set; } = null!;

    public Guid BrandId { get; set; }
    public CatalogBrandDto Brand { get; set; } = null!;
}
