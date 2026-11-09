import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { useState, type FormEvent } from 'react';
import { api } from '../api';
import { formatDateTime, formatHours } from '../lib/format';
import { maintenanceTypeLabels } from '../lib/labels';
import type { Drone, MaintenanceType } from '../types';
import { EmptyState, ErrorState, FormError, FormField, LoadingState, SectionTitle, fieldError } from './ui';

const maintenanceTypes = Object.keys(maintenanceTypeLabels) as MaintenanceType[];

export function MaintenanceSection({ drone }: { drone: Drone }) {
  const queryClient = useQueryClient();
  const [type, setType] = useState<MaintenanceType>('Scheduled');
  const [description, setDescription] = useState('');
  const [performedBy, setPerformedBy] = useState('');

  const history = useQuery({
    queryKey: ['maintenance-history', drone.id],
    queryFn: () => api.getMaintenanceHistory(drone.id),
  });

  const create = useMutation({
    mutationFn: () => api.createMaintenance(drone.id, { type, description, performedBy }),
    onSuccess: () => {
      setDescription('');
      setPerformedBy('');
      void queryClient.invalidateQueries();
    },
  });

  const isFlying = drone.status === 'InFlight';

  const submit = (event: FormEvent) => {
    event.preventDefault();
    create.mutate();
  };

  return (
    <section>
      <SectionTitle>Bakım Geçmişi</SectionTitle>
      {history.isPending && <LoadingState />}
      {history.error && <ErrorState error={history.error} />}
      {history.data?.length === 0 && <EmptyState text="Bakım kaydı yok." />}
      {history.data && history.data.length > 0 && (
        <ul className="divide-y divide-ops-800 border-y border-ops-800 text-sm">
          {history.data.map((record) => (
            <li key={record.id} className="px-3 py-2">
              <div className="flex justify-between gap-3 text-xs text-ops-500">
                <span>
                  {maintenanceTypeLabels[record.type]} · {record.performedBy}
                </span>
                <span className="whitespace-nowrap">{formatDateTime(record.performedAt)}</span>
              </div>
              <p>{record.description}</p>
              <p className="font-mono text-xs tabular-nums text-ops-500">
                Bakım anında toplam uçuş: {formatHours(record.flightMinutesAtService)}
              </p>
            </li>
          ))}
        </ul>
      )}

      <form onSubmit={submit} className="space-y-3 px-3 py-4" aria-label="Bakım Kaydı Ekle">
        <h3 className="text-sm font-medium">Bakım Kaydı Ekle</h3>
        <FormField label="Tip">
          <select className="form-control" value={type} onChange={(e) => setType(e.target.value as MaintenanceType)}>
            {maintenanceTypes.map((value) => (
              <option key={value} value={value}>
                {maintenanceTypeLabels[value]}
              </option>
            ))}
          </select>
        </FormField>
        <FormField label="Açıklama" error={fieldError(create.error, 'Description')}>
          <textarea className="form-control" rows={2} value={description} onChange={(e) => setDescription(e.target.value)} />
        </FormField>
        <FormField label="Yapan" error={fieldError(create.error, 'PerformedBy')}>
          <input className="form-control" value={performedBy} onChange={(e) => setPerformedBy(e.target.value)} />
        </FormField>
        <FormError error={create.error} />
        {isFlying && <p className="text-xs text-amber-300">Uçuştaki İHA için bakım kaydı eklenemez.</p>}
        <button type="submit" className="btn" disabled={create.isPending || isFlying}>
          {create.isPending ? 'Kaydediliyor…' : 'Kaydet'}
        </button>
      </form>
    </section>
  );
}
