import { useQuery } from '@tanstack/react-query';
import { api } from '../api';
import { formatNumber } from '../lib/format';
import { ErrorState, LoadingState } from './ui';

export function KpiStrip() {
  const { data, isPending, error } = useQuery({ queryKey: ['summary'], queryFn: api.getSummary });

  if (isPending) return <LoadingState />;
  if (error) return <ErrorState error={error} />;

  const items = [
    { label: 'Aktif İHA', value: formatNumber(data.activeDrones) },
    { label: 'Bugünkü Uçuş', value: formatNumber(data.flightsToday) },
    { label: 'İlaçlanan Alan', value: formatNumber(data.hectaresToday, 1), unit: 'ha' },
    { label: 'Kullanılan İlaç', value: formatNumber(data.liquidTodayLiters, 1), unit: 'L' },
    { label: 'Bakım Bekleyen', value: formatNumber(data.maintenanceDueCount), alert: data.maintenanceDueCount > 0 },
    { label: 'Açık Talep', value: formatNumber(data.openTickets) },
  ];

  return (
    <dl className="grid grid-cols-2 divide-x divide-ops-700 border-b border-ops-700 sm:grid-cols-3 xl:grid-cols-6">
      {items.map((item) => (
        <div key={item.label} className="px-4 py-2.5">
          <dt className="text-[11px] uppercase tracking-wide text-ops-500">{item.label}</dt>
          <dd className={`font-mono text-xl tabular-nums ${item.alert ? 'text-amber-300' : 'text-ops-100'}`}>
            {item.value}
            {item.unit && <span className="ml-1 text-xs text-ops-500">{item.unit}</span>}
          </dd>
        </div>
      ))}
    </dl>
  );
}
