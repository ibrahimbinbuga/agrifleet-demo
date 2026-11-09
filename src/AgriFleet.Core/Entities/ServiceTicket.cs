using AgriFleet.Core.Enums;

namespace AgriFleet.Core.Entities;

public class ServiceTicket
{
    public Guid Id { get; set; }
    public Guid DroneId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public TicketPriority Priority { get; set; }
    public TicketStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ResolvedAt { get; set; }

    public Drone Drone { get; set; } = null!;
}
