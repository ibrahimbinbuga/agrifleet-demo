import { useQuery } from '@tanstack/react-query';
import { useState } from 'react';
import { Link } from 'react-router-dom';
import { api } from '../api';
import { formatDateTime, formatNumber } from '../lib/format';
import { flightStatusLabels, flightStatusTones } from '../lib/labels';
import { Badge, EmptyState, ErrorState, LoadingState, Pager, SectionTitle } from './ui';

const PAGE_SIZE = 8;

export function FlightHistory({ droneId }: { droneId: string }) {
  const [page, setPage] = useState(1);
  const { data, isPending, error } = useQuery({
    queryKey: ['flights', droneId, page],
    queryFn: () => api.getFlights({ droneId, page, pageSize: PAGE_SIZE }),
  });

  return (
    <section>
      <SectionTitle>Uçuş Geçmişi</SectionTitle>
      {isPending && <LoadingState />}
      {error && <ErrorState error={error} />}
      {data?.items.length === 0 && <EmptyState text="Bu İHA için uçuş kaydı yok." />}
      {data && data.items.length > 0 && (
        <>
          <div className="overflow-x-auto">
            <table className="data-table">
              <thead>
                <tr>
                  <th>Başlangıç</th>
                  <th>Tarla</th>
                  <th>Süre</th>
                  <th>Alan</th>
                  <th>Durum</th>
                </tr>
              </thead>
              <tbody>
                {data.items.map((flight) => (
                  <tr key={flight.id}>
                    <td className="whitespace-nowrap">
                      <Link className="table-link" to={`/flights/${flight.id}`}>
                        {formatDateTime(flight.startedAt)}
                      </Link>
                    </td>
                    <td>{flight.fieldName}</td>
                    <td className="font-mono tabular-nums">{formatNumber(flight.durationMinutes, 1)} dk</td>
                    <td className="font-mono tabular-nums">{formatNumber(flight.sprayedHectares, 2)} ha</td>
                    <td>
                      <Badge tone={flightStatusTones[flight.status]}>{flightStatusLabels[flight.status]}</Badge>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
          <Pager page={data.page} pageSize={data.pageSize} totalCount={data.totalCount} onPageChange={setPage} />
        </>
      )}
    </section>
  );
}
