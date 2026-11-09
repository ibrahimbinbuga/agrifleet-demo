import { useQuery } from '@tanstack/react-query';
import { Link } from 'react-router-dom';
import { api } from '../api';
import { Badge, EmptyState, ErrorState, LoadingState, PageHeader } from '../components/ui';
import { formatDateTime, formatHours } from '../lib/format';
import { droneStatusLabels, droneStatusTones, maintenanceStatusLabels, maintenanceStatusTones } from '../lib/labels';

export function MaintenancePage() {
  const { data: drones, isPending, error } = useQuery({ queryKey: ['maintenance-due'], queryFn: api.getMaintenanceDue });

  return (
    <>
      <PageHeader title="Bakım Planı" subtitle="Bakımı yaklaşan ve geciken İHA'lar, aciliyet sırasıyla" />
      {isPending && <LoadingState />}
      {error && <ErrorState error={error} />}
      {drones?.length === 0 && <EmptyState text="Bakımı yaklaşan İHA yok." />}
      {drones && drones.length > 0 && (
        <div className="overflow-x-auto">
          <table className="data-table">
            <thead>
              <tr>
                <th>Seri No</th>
                <th>Bakım</th>
                <th>Neden</th>
                <th>Bakımdan Beri</th>
                <th>Son Bakım</th>
                <th>Durum</th>
              </tr>
            </thead>
            <tbody>
              {drones.map((drone) => (
                <tr key={drone.id}>
                  <td>
                    <Link className="table-link" to={`/drones/${drone.id}`}>
                      {drone.serialNumber}
                    </Link>
                  </td>
                  <td>
                    <Badge tone={maintenanceStatusTones[drone.maintenanceStatus]}>
                      {maintenanceStatusLabels[drone.maintenanceStatus]}
                    </Badge>
                  </td>
                  <td>{drone.maintenanceReason}</td>
                  <td className="font-mono tabular-nums">{formatHours(drone.flightMinutesSinceMaintenance)}</td>
                  <td className="whitespace-nowrap">{formatDateTime(drone.lastMaintenanceAt)}</td>
                  <td>
                    <Badge tone={droneStatusTones[drone.status]}>{droneStatusLabels[drone.status]}</Badge>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </>
  );
}
