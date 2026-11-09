using AgriFleet.Core.Enums;

namespace AgriFleet.Core.Dtos;

public record ServiceTicketDto(
    Guid Id,
    Guid DroneId,
    string DroneSerialNumber,
    string Title,
    string Description,
    TicketPriority Priority,
    TicketStatus Status,
    DateTime CreatedAt,
    DateTime? ResolvedAt);

public record CreateServiceTicketRequest(
    Guid DroneId,
    string Title,
    string Description,
    TicketPriority Priority);

public record UpdateServiceTicketStatusRequest(TicketStatus Status);
