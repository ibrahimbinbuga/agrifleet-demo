import { useQuery } from '@tanstack/react-query';
import { Link } from 'react-router-dom';
import { api } from '../api';
import { Badge, EmptyState, ErrorState, LoadingState, PageHeader } from '../components/ui';
import { formatDateTime, formatHours, formatNumber } from '../lib/format';
import { droneStatusLabels, droneStatusTones, maintenanceStatusLabels, maintenanceStatusTones } from '../lib/labels';

export function DronesPage() {
  const { data: drones, isPending, error } = useQuery({ queryKey: ['drones'], queryFn: api.getDrones });

  return (
    <>
      <PageHeader title="Filo" subtitle="Tüm İHA'ların durum ve bakım özeti" />
      {isPending && <LoadingState />}
      {error && <ErrorState error={error} />}
      {drones?.length === 0 && <EmptyState text="Kayıtlı İHA yok." />}
      {drones && drones.length > 0 && (
        <div className="overflow-x-auto">
          <table className="data-table">
            <thead>
              <tr>
                <th>Seri No</th>
                <th>Model</th>
                <th>Durum</th>
                <th>Bakım</th>
                <th>Toplam Uçuş</th>
                <th>Bakımdan Beri</th>
                <th>Batarya Döngüsü</th>
                <th>Son Bakım</th>
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
                  <td>{drone.model}</td>
                  <td>
                    <Badge tone={droneStatusTones[drone.status]}>{droneStatusLabels[drone.status]}</Badge>
                  </td>
                  <td title={drone.maintenanceReason}>
                    <Badge tone={maintenanceStatusTones[drone.maintenanceStatus]}>
                      {maintenanceStatusLabels[drone.maintenanceStatus]}
                    </Badge>
                  </td>
                  <td className="font-mono tabular-nums">{formatHours(drone.totalFlightMinutes)}</td>
                  <td className="font-mono tabular-nums">{formatHours(drone.flightMinutesSinceMaintenance)}</td>
                  <td className="font-mono tabular-nums">{formatNumber(drone.batteryCycles)}</td>
                  <td className="whitespace-nowrap">{formatDateTime(drone.lastMaintenanceAt)}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </>
  );
}
