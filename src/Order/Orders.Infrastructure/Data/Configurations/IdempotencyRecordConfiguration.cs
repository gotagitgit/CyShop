using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Orders.Infrastructure.Data.Entities;

namespace Orders.Infrastructure.Data.Configurations;

public class IdempotencyRecordConfiguration : IEntityTypeConfiguration<IdempotencyRecord>
{
    public void Configure(EntityTypeBuilder<IdempotencyRecord> builder)
    {
        builder.ToTable("IdempotencyRecords");
        builder.HasKey(r => r.IdempotencyKey);
        builder.Property(r => r.IdempotencyKey).ValueGeneratedNever();
        builder.Property(r => r.Timestamp).HasColumnType("timestamp with time zone");
    }
}
