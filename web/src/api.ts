import type {
  CreateMaintenanceRecord,
  CreateServiceTicket,
  DashboardSummary,
  Drone,
  Field,
  Flight,
  MaintenanceRecord,
  PagedResult,
  ServiceTicket,
  TelemetryPoint,
  TicketStatus,
} from './types';

interface ProblemDetails {
  title?: string;
  errors?: Record<string, string[]>;
}

export class ApiError extends Error {
  constructor(
    message: string,
    readonly status: number,
    readonly fieldErrors: Record<string, string[]> = {},
  ) {
    super(message);
  }
}

async function request<T>(path: string, init?: RequestInit): Promise<T> {
  const response = await fetch(path, {
    ...init,
    headers: init?.body ? { 'Content-Type': 'application/json' } : undefined,
  });

  if (!response.ok) {
    const problem = (await response.json().catch(() => null)) as ProblemDetails | null;
    throw new ApiError(problem?.title ?? 'Beklenmeyen bir hata oluştu.', response.status, problem?.errors);
  }

  return (await response.json()) as T;
}

function queryString(params: Record<string, string | number | undefined>): string {
  const entries = Object.entries(params).filter(([, value]) => value !== undefined && value !== '');
  return entries.length === 0 ? '' : `?${new URLSearchParams(entries.map(([k, v]) => [k, String(v)]))}`;
}

const postJson = (body: unknown): RequestInit => ({ method: 'POST', body: JSON.stringify(body) });
const patchJson = (body: unknown): RequestInit => ({ method: 'PATCH', body: JSON.stringify(body) });

export const api = {
  getSummary: () => request<DashboardSummary>('/api/dashboard/summary'),
  getDrones: () => request<Drone[]>('/api/drones'),
  getDrone: (id: string) => request<Drone>(`/api/drones/${id}`),
  getFields: () => request<Field[]>('/api/fields'),
  getFlights: (filter: { droneId?: string; page?: number; pageSize?: number }) =>
    request<PagedResult<Flight>>(`/api/flights${queryString(filter)}`),
  getFlight: (id: string) => request<Flight>(`/api/flights/${id}`),
  getFlightTelemetry: (id: string) => request<TelemetryPoint[]>(`/api/flights/${id}/telemetry`),
  getMaintenanceDue: () => request<Drone[]>('/api/maintenance/due'),
  getMaintenanceHistory: (droneId: string) => request<MaintenanceRecord[]>(`/api/drones/${droneId}/maintenance`),
  createMaintenance: (droneId: string, record: CreateMaintenanceRecord) =>
    request<MaintenanceRecord>(`/api/drones/${droneId}/maintenance`, postJson(record)),
  getTickets: (filter: { status?: TicketStatus; droneId?: string }) =>
    request<ServiceTicket[]>(`/api/service-tickets${queryString(filter)}`),
  createTicket: (ticket: CreateServiceTicket) => request<ServiceTicket>('/api/service-tickets', postJson(ticket)),
  updateTicketStatus: (id: string, status: TicketStatus) =>
    request<ServiceTicket>(`/api/service-tickets/${id}/status`, patchJson({ status })),
};
