export type DroneStatus = 'Idle' | 'InFlight' | 'Charging' | 'Maintenance' | 'Offline';
export type FlightStatus = 'InProgress' | 'Completed' | 'Aborted';
export type MaintenanceType = 'Scheduled' | 'Repair' | 'Inspection';
export type MaintenanceStatus = 'Ok' | 'DueSoon' | 'Overdue';
export type TicketPriority = 'Low' | 'Medium' | 'High' | 'Critical';
export type TicketStatus = 'Open' | 'InProgress' | 'Resolved';
export type AlertSeverity = 'Info' | 'Warning' | 'Critical';
export type FlightPhase = 'Takeoff' | 'Transit' | 'Spraying' | 'ReturnToBase' | 'Landing' | 'Charging' | 'Finished';

export interface DashboardSummary {
  activeDrones: number;
  flightsToday: number;
  hectaresToday: number;
  liquidTodayLiters: number;
  maintenanceDueCount: number;
  openTickets: number;
}

export interface Drone {
  id: string;
  serialNumber: string;
  model: string;
  status: DroneStatus;
  tankCapacityLiters: number;
  totalFlightMinutes: number;
  flightMinutesSinceMaintenance: number;
  batteryCycles: number;
  lastMaintenanceAt: string;
  createdAt: string;
  maintenanceStatus: MaintenanceStatus;
  maintenanceReason: string;
}

export interface Field {
  id: string;
  name: string;
  cropType: string;
  areaHectares: number;
  boundary: [number, number][];
}

export interface Flight {
  id: string;
  droneId: string;
  droneSerialNumber: string;
  fieldId: string;
  fieldName: string;
  startedAt: string;
  endedAt: string | null;
  durationMinutes: number;
  sprayedHectares: number;
  liquidUsedLiters: number;
  maxAltitudeM: number;
  status: FlightStatus;
  abortReason: string | null;
}

export interface PagedResult<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
}

export interface TelemetryPoint {
  timestamp: string;
  lat: number;
  lng: number;
  altitudeM: number;
  speedMs: number;
  batteryPct: number;
  tankLevelPct: number;
}

export interface MaintenanceRecord {
  id: string;
  droneId: string;
  type: MaintenanceType;
  description: string;
  performedBy: string;
  performedAt: string;
  flightMinutesAtService: number;
}

export interface CreateMaintenanceRecord {
  type: MaintenanceType;
  description: string;
  performedBy: string;
}

export interface ServiceTicket {
  id: string;
  droneId: string;
  droneSerialNumber: string;
  title: string;
  description: string;
  priority: TicketPriority;
  status: TicketStatus;
  createdAt: string;
  resolvedAt: string | null;
}

export interface CreateServiceTicket {
  droneId: string;
  title: string;
  description: string;
  priority: TicketPriority;
}

export interface LiveTelemetry extends TelemetryPoint {
  droneId: string;
  serialNumber: string;
  flightId: string;
  phase: FlightPhase;
}

export interface DroneStatusChange {
  droneId: string;
  status: DroneStatus;
}

export interface DroneAlert {
  droneId: string;
  serialNumber: string;
  severity: AlertSeverity;
  message: string;
  timestamp: string;
}
