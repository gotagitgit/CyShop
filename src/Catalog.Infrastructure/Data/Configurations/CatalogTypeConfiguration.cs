using Catalog.Infrastructure.Data.Dtos;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Catalog.Infrastructure.Data.Configurations;

public class CatalogTypeConfiguration : IEntityTypeConfiguration<CatalogTypeDto>
{
    public void Configure(EntityTypeBuilder<CatalogTypeDto> builder)
    {
        builder.ToTable("CatalogTypes");
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id).ValueGeneratedNever();
        builder.Property(t => t.Name).IsRequired().HasMaxLength(100);
    }
}
