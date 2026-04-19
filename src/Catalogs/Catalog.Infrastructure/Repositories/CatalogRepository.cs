using Catalog.Domain.Entities;
using Catalog.Domain.Interfaces;
using Catalog.Infrastructure.Data;
using Catalog.Infrastructure.Data.Dtos;
using Catalog.Infrastructure.Data.Mappers;
using Microsoft.EntityFrameworkCore;

namespace Catalog.Infrastructure.Repositories;

public class CatalogRepository(CatalogDbContext context) : ICatalogRepository
{
    public async Task<IReadOnlyList<CatalogItem>> GetAllAsync(CancellationToken ct = default)
    {
        var dtos = await context.CatalogItems
            .AsNoTracking()
            .Include(c => c.Type)
            .Include(c => c.Brand)
            .ToListAsync(ct);

        return dtos.Select(CatalogMapper.ToDomain).ToList();
    }

    public async Task<CatalogItem?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var dto = await context.CatalogItems
            .AsNoTracking()
            .Include(c => c.Type)
            .Include(c => c.Brand)
            .FirstOrDefaultAsync(c => c.Id == id, ct);

        return dto is null ? null : CatalogMapper.ToDomain(dto);
    }

    public async Task<IReadOnlyList<CatalogItem>> GetByTypeAsync(Guid typeId, CancellationToken ct = default)
    {
        var dtos = await context.CatalogItems
            .AsNoTracking()
            .Include(c => c.Type)
            .Include(c => c.Brand)
            .Where(c => c.TypeId == typeId)
            .ToListAsync(ct);

        return dtos.Select(CatalogMapper.ToDomain).ToList();
    }

    public async Task<IReadOnlyList<CatalogItem>> GetByBrandAsync(Guid brandId, CancellationToken ct = default)
    {
        var dtos = await context.CatalogItems
            .AsNoTracking()
            .Include(c => c.Type)
            .Include(c => c.Brand)
            .Where(c => c.BrandId == brandId)
            .ToListAsync(ct);

        return dtos.Select(CatalogMapper.ToDomain).ToList();
    }

    public async Task<bool> AnyAsync(CancellationToken ct = default) =>
        await context.CatalogItems.AnyAsync(ct);

    public async Task AddRangeAsync(IEnumerable<CatalogItem> items, CancellationToken ct = default)
    {
        foreach (var item in items)
        {
            var brandDto = CatalogMapper.ToDto(item.Brand);
            var typeDto = CatalogMapper.ToDto(item.Type);

            // Upsert brand and type to avoid duplicates
            var existingBrand = await context.CatalogBrands.FindAsync([brandDto.Id], ct);
            if (existingBrand is null)
                context.CatalogBrands.Add(brandDto);

            var existingType = await context.CatalogTypes.FindAsync([typeDto.Id], ct);
            if (existingType is null)
                context.CatalogTypes.Add(typeDto);

            var itemDto = CatalogMapper.ToDto(item);
            context.CatalogItems.Add(itemDto);
        }
    }

    public async Task<CatalogItem> AddAsync(CatalogItem item, CancellationToken ct = default)
    {
        var itemDto = CatalogMapper.ToDto(item);
        itemDto.TypeId = item.Type.id;
        itemDto.BrandId = item.Brand.id;
        context.CatalogItems.Add(itemDto);
        await context.SaveChangesAsync(ct);
        return item;
    }

    public async Task UpdateAsync(CatalogItem item, CancellationToken ct = default)
    {
        var existing = await context.CatalogItems.FindAsync([item.Id], ct)
            ?? throw new InvalidOperationException($"Catalog item {item.Id} not found");

        existing.Name = item.Name;
        existing.Description = item.Description;
        existing.Price = item.Price;
        existing.ImagePath = item.ImagePath;
        existing.TypeId = item.Type.id;
        existing.BrandId = item.Brand.id;
        await context.SaveChangesAsync(ct);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var existing = await context.CatalogItems.FindAsync([id], ct);
        if (existing is null) return false;
        context.CatalogItems.Remove(existing);
        await context.SaveChangesAsync(ct);
        return true;
    }

    public async Task<CatalogBrand> FindOrCreateBrandAsync(string name, CancellationToken ct = default)
    {
        var existing = await context.CatalogBrands
            .FirstOrDefaultAsync(b => b.Name == name, ct);

        if (existing is not null)
            return new CatalogBrand(existing.Id, existing.Name);

        var brandDto = new CatalogBrandDto { Id = Guid.NewGuid(), Name = name };
        context.CatalogBrands.Add(brandDto);
        await context.SaveChangesAsync(ct);
        return new CatalogBrand(brandDto.Id, brandDto.Name);
    }

    public async Task<CatalogType> FindOrCreateTypeAsync(string name, CancellationToken ct = default)
    {
        var existing = await context.CatalogTypes
            .FirstOrDefaultAsync(t => t.Name == name, ct);

        if (existing is not null)
            return new CatalogType(existing.Id, existing.Name);

        var typeDto = new CatalogTypeDto { Id = Guid.NewGuid(), Name = name };
        context.CatalogTypes.Add(typeDto);
        await context.SaveChangesAsync(ct);
        return new CatalogType(typeDto.Id, typeDto.Name);
    }

    public async Task SaveChangesAsync(CancellationToken ct = default) =>
        await context.SaveChangesAsync(ct);
}
