import { useQuery } from '@tanstack/react-query';
import { useParams } from 'react-router-dom';
import { api } from '../api';
import { FlightHistory } from '../components/FlightHistory';
import { MaintenanceSection } from '../components/MaintenanceSection';
import { Badge, EmptyState, ErrorState, LoadingState, PageHeader, SectionTitle } from '../components/ui';
import { formatDateTime, formatHours, formatNumber } from '../lib/format';
import {
  droneStatusLabels,
  droneStatusTones,
  maintenanceStatusLabels,
  maintenanceStatusTones,
  ticketPriorityLabels,
  ticketPriorityTones,
  ticketStatusLabels,
  ticketStatusTones,
} from '../lib/labels';

export function DroneDetailPage() {
  const { id = '' } = useParams();
  const { data: drone, isPending, error } = useQuery({ queryKey: ['drones', id], queryFn: () => api.getDrone(id) });
  const tickets = useQuery({ queryKey: ['tickets', { droneId: id }], queryFn: () => api.getTickets({ droneId: id }) });

  if (isPending) return <LoadingState />;
  if (error) return <ErrorState error={error} />;

  const facts = [
    { label: 'Model', value: drone.model },
    { label: 'Tank', value: `${formatNumber(drone.tankCapacityLiters)} L` },
    { label: 'Toplam Uçuş', value: formatHours(drone.totalFlightMinutes) },
    { label: 'Bakımdan Beri', value: formatHours(drone.flightMinutesSinceMaintenance) },
    { label: 'Batarya Döngüsü', value: formatNumber(drone.batteryCycles) },
    { label: 'Son Bakım', value: formatDateTime(drone.lastMaintenanceAt) },
  ];

  return (
    <>
      <PageHeader title={drone.serialNumber} subtitle={drone.maintenanceReason}>
        <div className="flex gap-2">
          <Badge tone={droneStatusTones[drone.status]}>{droneStatusLabels[drone.status]}</Badge>
          <Badge tone={maintenanceStatusTones[drone.maintenanceStatus]}>
            Bakım: {maintenanceStatusLabels[drone.maintenanceStatus]}
          </Badge>
        </div>
      </PageHeader>

      <dl className="grid grid-cols-2 divide-x divide-ops-700 border-b border-ops-700 md:grid-cols-3 xl:grid-cols-6">
        {facts.map((fact) => (
          <div key={fact.label} className="px-4 py-2.5">
            <dt className="text-[11px] uppercase tracking-wide text-ops-500">{fact.label}</dt>
            <dd className="font-mono text-sm tabular-nums">{fact.value}</dd>
          </div>
        ))}
      </dl>

      <div className="grid grid-cols-1 gap-x-6 p-3 xl:grid-cols-[minmax(0,3fr)_minmax(0,2fr)]">
        <div>
          <FlightHistory droneId={drone.id} />
          <section className="mt-4">
            <SectionTitle>Servis Talepleri</SectionTitle>
            {tickets.isPending && <LoadingState />}
            {tickets.error && <ErrorState error={tickets.error} />}
            {tickets.data?.length === 0 && <EmptyState text="Bu İHA için servis talebi yok." />}
            {tickets.data && tickets.data.length > 0 && (
              <ul className="divide-y divide-ops-800 border-y border-ops-800 text-sm">
                {tickets.data.map((ticket) => (
                  <li key={ticket.id} className="flex flex-wrap items-center justify-between gap-2 px-3 py-2">
                    <span>{ticket.title}</span>
                    <span className="flex gap-2">
                      <Badge tone={ticketPriorityTones[ticket.priority]}>{ticketPriorityLabels[ticket.priority]}</Badge>
                      <Badge tone={ticketStatusTones[ticket.status]}>{ticketStatusLabels[ticket.status]}</Badge>
                    </span>
                  </li>
                ))}
              </ul>
            )}
          </section>
        </div>
        <MaintenanceSection drone={drone} />
      </div>
    </>
  );
}
