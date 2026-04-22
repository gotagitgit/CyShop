using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Orders.Infrastructure.Data.Entities;

namespace Orders.Infrastructure.Data.Configurations;

public class OrderConfiguration : IEntityTypeConfiguration<OrderEntity>
{
    public void Configure(EntityTypeBuilder<OrderEntity> builder)
    {
        builder.ToTable("Orders");
        builder.HasKey(o => o.Id);
        builder.Property(o => o.Id).ValueGeneratedNever();
        builder.Property(o => o.CustomerName).IsRequired().HasMaxLength(200);
        builder.Property(o => o.Status).IsRequired().HasMaxLength(50);
        builder.Property(o => o.OrderDate).HasColumnType("timestamp with time zone");

        // Shipping address as owned columns
        builder.Property(o => o.Street).IsRequired().HasMaxLength(200);
        builder.Property(o => o.City).IsRequired().HasMaxLength(100);
        builder.Property(o => o.State).IsRequired().HasMaxLength(100);
        builder.Property(o => o.Country).IsRequired().HasMaxLength(100);
        builder.Property(o => o.ZipCode).IsRequired().HasMaxLength(20);

        builder.HasMany(o => o.Items)
            .WithOne(i => i.Order)
            .HasForeignKey(i => i.OrderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
