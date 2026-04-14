using Catalog.Infrastructure.Data.Dtos;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Catalog.Infrastructure.Data.Configurations;

public class CatalogItemConfiguration : IEntityTypeConfiguration<CatalogItemDto>
{
    public void Configure(EntityTypeBuilder<CatalogItemDto> builder)
    {
        builder.ToTable("CatalogItems");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).ValueGeneratedNever();
        builder.Property(c => c.Name).IsRequired().HasMaxLength(200);
        builder.Property(c => c.Description).IsRequired().HasMaxLength(2000);
        builder.Property(c => c.Price).HasPrecision(18, 2);
        builder.Property(c => c.ImagePath).IsRequired().HasMaxLength(200);

        builder.HasOne(c => c.Type)
            .WithMany(t => t.CatalogItems)
            .HasForeignKey(c => c.TypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.Brand)
            .WithMany(b => b.CatalogItems)
            .HasForeignKey(c => c.BrandId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
