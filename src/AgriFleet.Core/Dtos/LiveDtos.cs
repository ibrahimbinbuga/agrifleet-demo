using AgriFleet.Core.Enums;
using AgriFleet.Core.Simulation;

namespace AgriFleet.Core.Dtos;

public record LiveTelemetryDto(
    Guid DroneId,
    string SerialNumber,
    Guid FlightId,
    FlightPhase Phase,
    DateTime Timestamp,
    double Lat,
    double Lng,
    double AltitudeM,
    double SpeedMs,
    double BatteryPct,
    double TankLevelPct);

public record DroneStatusChangedDto(Guid DroneId, DroneStatus Status);

public record DroneAlertDto(Guid DroneId, string SerialNumber, AlertSeverity Severity, string Message, DateTime Timestamp);
