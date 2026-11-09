using AgriFleet.Core.Entities;
using AgriFleet.Core.Enums;
using Microsoft.EntityFrameworkCore;

namespace AgriFleet.Infrastructure.Data;

public class AgriFleetDbContext(DbContextOptions<AgriFleetDbContext> options) : DbContext(options)
{
    public DbSet<Drone> Drones => Set<Drone>();
    public DbSet<Field> Fields => Set<Field>();
    public DbSet<Flight> Flights => Set<Flight>();
    public DbSet<TelemetryPoint> TelemetryPoints => Set<TelemetryPoint>();
    public DbSet<MaintenanceRecord> MaintenanceRecords => Set<MaintenanceRecord>();
    public DbSet<ServiceTicket> ServiceTickets => Set<ServiceTicket>();

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Properties<DroneStatus>().HaveConversion<string>().HaveMaxLength(20);
        configurationBuilder.Properties<FlightStatus>().HaveConversion<string>().HaveMaxLength(20);
        configurationBuilder.Properties<MaintenanceType>().HaveConversion<string>().HaveMaxLength(20);
        configurationBuilder.Properties<TicketPriority>().HaveConversion<string>().HaveMaxLength(20);
        configurationBuilder.Properties<TicketStatus>().HaveConversion<string>().HaveMaxLength(20);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AgriFleetDbContext).Assembly);

        foreach (var foreignKey in modelBuilder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()))
            foreignKey.DeleteBehavior = DeleteBehavior.Restrict;
    }
}
