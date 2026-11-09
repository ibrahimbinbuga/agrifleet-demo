using AgriFleet.Core.Enums;

namespace AgriFleet.Core.Entities;

public class Flight
{
    public Guid Id { get; set; }
    public Guid DroneId { get; set; }
    public Guid FieldId { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? EndedAt { get; set; }
    public double DurationMinutes { get; set; }
    public double SprayedHectares { get; set; }
    public double LiquidUsedLiters { get; set; }
    public double MaxAltitudeM { get; set; }
    public FlightStatus Status { get; set; }
    public string? AbortReason { get; set; }

    public Drone Drone { get; set; } = null!;
    public Field Field { get; set; } = null!;
    public List<TelemetryPoint> TelemetryPoints { get; set; } = new();
}
