namespace AgriFleet.Core.Entities;

public class TelemetryPoint
{
    public long Id { get; set; }
    public Guid DroneId { get; set; }
    public Guid FlightId { get; set; }
    public DateTime Timestamp { get; set; }
    public double Lat { get; set; }
    public double Lng { get; set; }
    public double AltitudeM { get; set; }
    public double SpeedMs { get; set; }
    public double BatteryPct { get; set; }
    public double TankLevelPct { get; set; }

    public Flight Flight { get; set; } = null!;
}
