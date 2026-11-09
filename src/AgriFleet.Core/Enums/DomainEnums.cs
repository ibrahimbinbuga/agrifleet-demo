namespace AgriFleet.Core.Enums;

public enum DroneStatus { Idle, InFlight, Charging, Maintenance, Offline }

public enum FlightStatus { InProgress, Completed, Aborted }

public enum MaintenanceType { Scheduled, Repair, Inspection }

public enum MaintenanceStatus { Ok, DueSoon, Overdue }

public enum TicketPriority { Low, Medium, High, Critical }

public enum TicketStatus { Open, InProgress, Resolved }

public enum AlertSeverity { Info, Warning, Critical }
