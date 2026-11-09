using AgriFleet.Core.Entities;
using AgriFleet.Core.Enums;
using AgriFleet.Core.Services;

namespace AgriFleet.Core.Dtos;

public record DroneDto(
    Guid Id,
    string SerialNumber,
    string Model,
    DroneStatus Status,
    double TankCapacityLiters,
    double TotalFlightMinutes,
    double FlightMinutesSinceMaintenance,
    int BatteryCycles,
    DateTime LastMaintenanceAt,
    DateTime CreatedAt,
    MaintenanceStatus MaintenanceStatus,
    string MaintenanceReason)
{
    public static DroneDto From(Drone drone, DateTime now)
    {
        var evaluation = MaintenancePolicy.Evaluate(drone, now);

        return new DroneDto(
            drone.Id,
            drone.SerialNumber,
            drone.Model,
            drone.Status,
            drone.TankCapacityLiters,
            drone.TotalFlightMinutes,
            drone.FlightMinutesSinceMaintenance,
            drone.BatteryCycles,
            drone.LastMaintenanceAt,
            drone.CreatedAt,
            evaluation.Status,
            evaluation.Reason);
    }
}

public record CreateDroneRequest(string SerialNumber, string Model, double TankCapacityLiters);

public record UpdateDroneStatusRequest(DroneStatus Status);
