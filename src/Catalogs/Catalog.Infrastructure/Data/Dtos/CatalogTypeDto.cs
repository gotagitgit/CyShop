namespace Catalog.Infrastructure.Data.Dtos;

public class CatalogTypeDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;

    public ICollection<CatalogItemDto> CatalogItems { get; set; } = [];
}
