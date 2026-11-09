using AgriFleet.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AgriFleet.Infrastructure.Data.Configurations;

public class FieldConfiguration : IEntityTypeConfiguration<Field>
{
    public void Configure(EntityTypeBuilder<Field> builder)
    {
        builder.Property(f => f.Name).IsRequired().HasMaxLength(128);
        builder.Property(f => f.CropType).IsRequired().HasMaxLength(64);
        builder.Property(f => f.BoundaryJson).IsRequired().HasColumnType("jsonb");
    }
}
