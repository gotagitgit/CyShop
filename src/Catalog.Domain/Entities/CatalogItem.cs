namespace Catalog.Domain.Entities;

public sealed record CatalogItem
{
    public CatalogItem(Guid id, CatalogType type, CatalogBrand brand, string name, string description, decimal price, string imagePath)
    {
        Id = id;
        Type = type;
        Brand = brand;
        Name = name;
        Description = description;
        Price = price;
        ImagePath = imagePath;
    }

    public Guid Id { get; init; }
    
    public string Name { get; init; } = string.Empty;

    public string Description { get; init; } = string.Empty;

    public decimal Price { get; init; }

    public string ImagePath { get; set; } = string.Empty;

    public CatalogType Type { get; init; }

    public CatalogBrand Brand { get; init; }
}
