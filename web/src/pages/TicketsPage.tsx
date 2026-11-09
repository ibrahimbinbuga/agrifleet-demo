import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { useState } from 'react';
import { Link } from 'react-router-dom';
import { api } from '../api';
import { CreateTicketForm } from '../components/CreateTicketForm';
import { Badge, EmptyState, ErrorState, FormError, LoadingState, PageHeader } from '../components/ui';
import { formatDateTime } from '../lib/format';
import { ticketPriorityLabels, ticketPriorityTones, ticketStatusLabels } from '../lib/labels';
import type { TicketStatus } from '../types';

const statuses = Object.keys(ticketStatusLabels) as TicketStatus[];

export function TicketsPage() {
  const queryClient = useQueryClient();
  const [filter, setFilter] = useState<TicketStatus | ''>('');
  const { data: tickets, isPending, error } = useQuery({
    queryKey: ['tickets', { status: filter }],
    queryFn: () => api.getTickets({ status: filter || undefined }),
  });

  const updateStatus = useMutation({
    mutationFn: ({ id, status }: { id: string; status: TicketStatus }) => api.updateTicketStatus(id, status),
    onSuccess: () => void queryClient.invalidateQueries(),
  });

  return (
    <>
      <PageHeader title="Teknik Servis" subtitle="Arıza ve servis talepleri">
        <label className="flex items-center gap-2 text-xs text-ops-300">
          Durum
          <select className="form-control w-36" value={filter} onChange={(e) => setFilter(e.target.value as TicketStatus | '')}>
            <option value="">Tümü</option>
            {statuses.map((status) => (
              <option key={status} value={status}>
                {ticketStatusLabels[status]}
              </option>
            ))}
          </select>
        </label>
      </PageHeader>

      <div className="grid grid-cols-1 xl:grid-cols-[minmax(0,1fr)_22rem]">
        <div className="min-w-0">
          <div className="px-3 pt-2">
            <FormError error={updateStatus.error} />
          </div>
          {isPending && <LoadingState />}
          {error && <ErrorState error={error} />}
          {tickets?.length === 0 && <EmptyState text="Bu filtreye uygun talep yok." />}
          {tickets && tickets.length > 0 && (
            <div className="overflow-x-auto">
              <table className="data-table">
                <thead>
                  <tr>
                    <th>Başlık</th>
                    <th>İHA</th>
                    <th>Öncelik</th>
                    <th>Oluşturulma</th>
                    <th>Durum</th>
                  </tr>
                </thead>
                <tbody>
                  {tickets.map((ticket) => (
                    <tr key={ticket.id}>
                      <td>
                        <p>{ticket.title}</p>
                        <p className="text-xs text-ops-500">{ticket.description}</p>
                      </td>
                      <td>
                        <Link className="table-link" to={`/drones/${ticket.droneId}`}>
                          {ticket.droneSerialNumber}
                        </Link>
                      </td>
                      <td>
                        <Badge tone={ticketPriorityTones[ticket.priority]}>{ticketPriorityLabels[ticket.priority]}</Badge>
                      </td>
                      <td className="whitespace-nowrap">{formatDateTime(ticket.createdAt)}</td>
                      <td>
                        <select
                          aria-label={`${ticket.title} durumu`}
                          className="form-control w-32"
                          value={ticket.status}
                          disabled={updateStatus.isPending}
                          onChange={(e) => updateStatus.mutate({ id: ticket.id, status: e.target.value as TicketStatus })}
                        >
                          {statuses.map((status) => (
                            <option key={status} value={status}>
                              {ticketStatusLabels[status]}
                            </option>
                          ))}
                        </select>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}
        </div>
        <aside className="border-t border-ops-700 bg-ops-900 xl:border-l xl:border-t-0">
          <CreateTicketForm />
        </aside>
      </div>
    </>
  );
}
