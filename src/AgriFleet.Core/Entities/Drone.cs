using AgriFleet.Core.Enums;

namespace AgriFleet.Core.Entities;

public class Drone
{
    public Guid Id { get; set; }
    public string SerialNumber { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public DroneStatus Status { get; set; }
    public double TankCapacityLiters { get; set; }
    public double TotalFlightMinutes { get; set; }
    public double FlightMinutesSinceMaintenance { get; set; }
    public int BatteryCycles { get; set; }
    public DateTime LastMaintenanceAt { get; set; }
    public DateTime CreatedAt { get; set; }

    public List<Flight> Flights { get; set; } = new();
    public List<MaintenanceRecord> MaintenanceRecords { get; set; } = new();
    public List<ServiceTicket> ServiceTickets { get; set; } = new();
}
