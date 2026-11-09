using AgriFleet.Core.Enums;

namespace AgriFleet.Core.Dtos;

public record MaintenanceRecordDto(
    Guid Id,
    Guid DroneId,
    MaintenanceType Type,
    string Description,
    string PerformedBy,
    DateTime PerformedAt,
    double FlightMinutesAtService);

public record CreateMaintenanceRecordRequest(MaintenanceType Type, string Description, string PerformedBy);
