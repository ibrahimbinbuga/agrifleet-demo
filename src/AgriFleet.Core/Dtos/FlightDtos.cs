using AgriFleet.Core.Enums;

namespace AgriFleet.Core.Dtos;

public record FlightDto(
    Guid Id,
    Guid DroneId,
    string DroneSerialNumber,
    Guid FieldId,
    string FieldName,
    DateTime StartedAt,
    DateTime? EndedAt,
    double DurationMinutes,
    double SprayedHectares,
    double LiquidUsedLiters,
    double MaxAltitudeM,
    FlightStatus Status,
    string? AbortReason);

public record TelemetryPointDto(
    DateTime Timestamp,
    double Lat,
    double Lng,
    double AltitudeM,
    double SpeedMs,
    double BatteryPct,
    double TankLevelPct);

public record PagedResult<T>(IReadOnlyList<T> Items, int Page, int PageSize, int TotalCount);
