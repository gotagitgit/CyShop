using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Orders.Application.IntegrationEvents;

namespace Orders.Infrastructure.Data.Configurations;

public class IntegrationEventLogConfiguration : IEntityTypeConfiguration<IntegrationEventLogEntry>
{
    public void Configure(EntityTypeBuilder<IntegrationEventLogEntry> builder)
    {
        builder.ToTable("IntegrationEventLog");
        builder.HasKey(e => e.EventId);
        builder.Property(e => e.EventId).ValueGeneratedNever();
        builder.Property(e => e.EventTypeName).IsRequired().HasMaxLength(500);
        builder.Property(e => e.Content).IsRequired();
        builder.Property(e => e.State).IsRequired();
        builder.Property(e => e.CreationTime).HasColumnType("timestamp with time zone");
        builder.Property(e => e.TimesSent).HasDefaultValue(0);

        builder.HasIndex(e => e.State);
    }
}
