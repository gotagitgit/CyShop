using Catalog.Infrastructure.Data.Configurations;
using Catalog.Infrastructure.Data.Dtos;
using Microsoft.EntityFrameworkCore;

namespace Catalog.Infrastructure.Data;

public class CatalogDbContext(DbContextOptions<CatalogDbContext> options) : DbContext(options)
{
    public DbSet<CatalogItemDto> CatalogItems => Set<CatalogItemDto>();
    public DbSet<CatalogBrandDto> CatalogBrands => Set<CatalogBrandDto>();
    public DbSet<CatalogTypeDto> CatalogTypes => Set<CatalogTypeDto>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new CatalogItemConfiguration());
        modelBuilder.ApplyConfiguration(new CatalogBrandConfiguration());
        modelBuilder.ApplyConfiguration(new CatalogTypeConfiguration());
    }
}
