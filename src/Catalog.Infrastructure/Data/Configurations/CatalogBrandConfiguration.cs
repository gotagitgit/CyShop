using Catalog.Infrastructure.Data.Dtos;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Catalog.Infrastructure.Data.Configurations;

public class CatalogBrandConfiguration : IEntityTypeConfiguration<CatalogBrandDto>
{
    public void Configure(EntityTypeBuilder<CatalogBrandDto> builder)
    {
        builder.ToTable("CatalogBrands");
        builder.HasKey(b => b.Id);
        builder.Property(b => b.Id).ValueGeneratedNever();
        builder.Property(b => b.Name).IsRequired().HasMaxLength(100);
    }
}
