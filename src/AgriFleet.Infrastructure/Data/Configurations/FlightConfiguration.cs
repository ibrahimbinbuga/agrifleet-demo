using AgriFleet.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AgriFleet.Infrastructure.Data.Configurations;

public class FlightConfiguration : IEntityTypeConfiguration<Flight>
{
    public void Configure(EntityTypeBuilder<Flight> builder)
    {
        builder.Property(f => f.AbortReason).HasMaxLength(256);
        builder.HasIndex(f => new { f.DroneId, f.StartedAt });
        builder.HasIndex(f => f.StartedAt);
    }
}
