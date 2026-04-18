using Customers.Infrastructure.Data.Dtos;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Customers.Infrastructure.Data.Configurations;

public class CustomerConfiguration : IEntityTypeConfiguration<CustomerDto>
{
    public void Configure(EntityTypeBuilder<CustomerDto> builder)
    {
        builder.ToTable("Customers");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).ValueGeneratedNever();
        builder.Property(c => c.ExternalId).IsRequired();
        builder.HasIndex(c => c.ExternalId).IsUnique();
        builder.Property(c => c.FirstName).IsRequired().HasMaxLength(100);
        builder.Property(c => c.LastName).IsRequired().HasMaxLength(100);
        builder.Property(c => c.Email).IsRequired().HasMaxLength(256);
        builder.Property(c => c.ContactNumber).IsRequired().HasMaxLength(50);
    }
}
