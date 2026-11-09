using AgriFleet.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AgriFleet.Infrastructure.Data.Configurations;

public class MaintenanceRecordConfiguration : IEntityTypeConfiguration<MaintenanceRecord>
{
    public void Configure(EntityTypeBuilder<MaintenanceRecord> builder)
    {
        builder.Property(m => m.Description).IsRequired().HasMaxLength(1000);
        builder.Property(m => m.PerformedBy).IsRequired().HasMaxLength(128);
        builder.HasIndex(m => new { m.DroneId, m.PerformedAt });
    }
}
