import { useQuery } from '@tanstack/react-query';
import { Link, useParams } from 'react-router-dom';
import { api } from '../api';
import { FlightCharts } from '../components/FlightCharts';
import { FlightRouteMap } from '../components/FlightRouteMap';
import { Badge, EmptyState, ErrorState, LoadingState, PageHeader } from '../components/ui';
import { formatDateTime, formatNumber } from '../lib/format';
import { flightStatusLabels, flightStatusTones } from '../lib/labels';

export function FlightDetailPage() {
  const { id = '' } = useParams();
  const { data: flight, isPending, error } = useQuery({ queryKey: ['flights', 'detail', id], queryFn: () => api.getFlight(id) });
  const telemetry = useQuery({ queryKey: ['flights', 'telemetry', id], queryFn: () => api.getFlightTelemetry(id) });
  const fields = useQuery({ queryKey: ['fields'], queryFn: api.getFields });

  if (isPending) return <LoadingState />;
  if (error) return <ErrorState error={error} />;

  const stats = [
    { label: 'Başlangıç', value: formatDateTime(flight.startedAt) },
    { label: 'Bitiş', value: flight.endedAt ? formatDateTime(flight.endedAt) : '—' },
    { label: 'Süre', value: `${formatNumber(flight.durationMinutes, 1)} dk` },
    { label: 'İlaçlanan Alan', value: `${formatNumber(flight.sprayedHectares, 2)} ha` },
    { label: 'Kullanılan İlaç', value: `${formatNumber(flight.liquidUsedLiters, 1)} L` },
    { label: 'Maks. İrtifa', value: `${formatNumber(flight.maxAltitudeM, 1)} m` },
  ];

  const points = telemetry.data ?? [];

  return (
    <>
      <PageHeader title={`Uçuş · ${flight.fieldName}`} subtitle={flight.abortReason ? `İptal nedeni: ${flight.abortReason}` : undefined}>
        <div className="flex items-center gap-3">
          <Link className="table-link text-sm" to={`/drones/${flight.droneId}`}>
            {flight.droneSerialNumber}
          </Link>
          <Badge tone={flightStatusTones[flight.status]}>{flightStatusLabels[flight.status]}</Badge>
        </div>
      </PageHeader>

      <dl className="grid grid-cols-2 divide-x divide-ops-700 border-b border-ops-700 md:grid-cols-3 xl:grid-cols-6">
        {stats.map((stat) => (
          <div key={stat.label} className="px-4 py-2.5">
            <dt className="text-[11px] uppercase tracking-wide text-ops-500">{stat.label}</dt>
            <dd className="font-mono text-sm tabular-nums">{stat.value}</dd>
          </div>
        ))}
      </dl>

      {telemetry.isPending && <LoadingState text="Rota yükleniyor…" />}
      {telemetry.error && <ErrorState error={telemetry.error} />}
      {telemetry.data && points.length === 0 && (
        <EmptyState text="Bu uçuş için telemetri kaydı tutulmamış. Telemetri yalnızca son uçuşlar için saklanır." />
      )}
      {points.length > 0 && (
        <div className="grid grid-cols-1 lg:grid-cols-[minmax(0,3fr)_minmax(0,2fr)]">
          <FlightRouteMap telemetry={points} field={fields.data?.find((field) => field.id === flight.fieldId)} />
          <FlightCharts telemetry={points} />
        </div>
      )}
    </>
  );
}
