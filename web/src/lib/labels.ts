import type {
  DroneStatus,
  FlightPhase,
  FlightStatus,
  MaintenanceStatus,
  MaintenanceType,
  TicketPriority,
  TicketStatus,
} from '../types';

export type Tone = 'green' | 'amber' | 'red' | 'blue' | 'neutral';

export const droneStatusLabels: Record<DroneStatus, string> = {
  Idle: 'Hazır',
  InFlight: 'Uçuşta',
  Charging: 'Şarjda',
  Maintenance: 'Bakımda',
  Offline: 'Çevrimdışı',
};

export const droneStatusTones: Record<DroneStatus, Tone> = {
  Idle: 'neutral',
  InFlight: 'green',
  Charging: 'blue',
  Maintenance: 'amber',
  Offline: 'red',
};

export const phaseLabels: Record<FlightPhase, string> = {
  Takeoff: 'Kalkış',
  Transit: 'Tarlaya gidiyor',
  Spraying: 'İlaçlama',
  ReturnToBase: 'Üsse dönüyor',
  Landing: 'İniş',
  Charging: 'Şarjda',
  Finished: 'Tamamlandı',
};

export const maintenanceStatusLabels: Record<MaintenanceStatus, string> = {
  Ok: 'Uygun',
  DueSoon: 'Yaklaşıyor',
  Overdue: 'Gecikmiş',
};

export const maintenanceStatusTones: Record<MaintenanceStatus, Tone> = {
  Ok: 'green',
  DueSoon: 'amber',
  Overdue: 'red',
};

export const maintenanceTypeLabels: Record<MaintenanceType, string> = {
  Scheduled: 'Periyodik',
  Repair: 'Onarım',
  Inspection: 'Muayene',
};

export const flightStatusLabels: Record<FlightStatus, string> = {
  InProgress: 'Devam ediyor',
  Completed: 'Tamamlandı',
  Aborted: 'İptal edildi',
};

export const flightStatusTones: Record<FlightStatus, Tone> = {
  InProgress: 'blue',
  Completed: 'green',
  Aborted: 'red',
};

export const ticketStatusLabels: Record<TicketStatus, string> = {
  Open: 'Açık',
  InProgress: 'İşlemde',
  Resolved: 'Çözüldü',
};

export const ticketStatusTones: Record<TicketStatus, Tone> = {
  Open: 'amber',
  InProgress: 'blue',
  Resolved: 'green',
};

export const ticketPriorityLabels: Record<TicketPriority, string> = {
  Low: 'Düşük',
  Medium: 'Orta',
  High: 'Yüksek',
  Critical: 'Kritik',
};

export const ticketPriorityTones: Record<TicketPriority, Tone> = {
  Low: 'neutral',
  Medium: 'blue',
  High: 'amber',
  Critical: 'red',
};
