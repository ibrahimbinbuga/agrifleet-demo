using AgriFleet.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AgriFleet.Infrastructure.Data.Configurations;

public class DroneConfiguration : IEntityTypeConfiguration<Drone>
{
    public void Configure(EntityTypeBuilder<Drone> builder)
    {
        builder.Property(d => d.SerialNumber).IsRequired().HasMaxLength(64);
        builder.Property(d => d.Model).IsRequired().HasMaxLength(32);
        builder.HasIndex(d => d.SerialNumber).IsUnique();
    }
}
