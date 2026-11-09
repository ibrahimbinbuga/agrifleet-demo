using AgriFleet.Core.Enums;

namespace AgriFleet.Core.Entities;

public class MaintenanceRecord
{
    public Guid Id { get; set; }
    public Guid DroneId { get; set; }
    public MaintenanceType Type { get; set; }
    public string Description { get; set; } = string.Empty;
    public string PerformedBy { get; set; } = string.Empty;
    public DateTime PerformedAt { get; set; }
    public double FlightMinutesAtService { get; set; }

    public Drone Drone { get; set; } = null!;
}
