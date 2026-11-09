import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { useState, type FormEvent } from 'react';
import { api } from '../api';
import { ticketPriorityLabels } from '../lib/labels';
import type { TicketPriority } from '../types';
import { FormError, FormField, fieldError } from './ui';

const priorities = Object.keys(ticketPriorityLabels) as TicketPriority[];

export function CreateTicketForm() {
  const queryClient = useQueryClient();
  const drones = useQuery({ queryKey: ['drones'], queryFn: api.getDrones });
  const [droneId, setDroneId] = useState('');
  const [title, setTitle] = useState('');
  const [description, setDescription] = useState('');
  const [priority, setPriority] = useState<TicketPriority>('Medium');

  const create = useMutation({
    mutationFn: () => api.createTicket({ droneId, title, description, priority }),
    onSuccess: () => {
      setTitle('');
      setDescription('');
      void queryClient.invalidateQueries();
    },
  });

  const submit = (event: FormEvent) => {
    event.preventDefault();
    create.mutate();
  };

  return (
    <form onSubmit={submit} className="space-y-3 p-4" aria-label="Yeni Talep">
      <h2 className="text-sm font-medium">Yeni Talep</h2>
      <FormField label="İHA">
        <select className="form-control" required value={droneId} onChange={(e) => setDroneId(e.target.value)}>
          <option value="">Seçiniz</option>
          {drones.data?.map((drone) => (
            <option key={drone.id} value={drone.id}>
              {drone.serialNumber}
            </option>
          ))}
        </select>
      </FormField>
      <FormField label="Başlık" error={fieldError(create.error, 'Title')}>
        <input className="form-control" value={title} onChange={(e) => setTitle(e.target.value)} />
      </FormField>
      <FormField label="Açıklama" error={fieldError(create.error, 'Description')}>
        <textarea className="form-control" rows={3} value={description} onChange={(e) => setDescription(e.target.value)} />
      </FormField>
      <FormField label="Öncelik">
        <select className="form-control" value={priority} onChange={(e) => setPriority(e.target.value as TicketPriority)}>
          {priorities.map((value) => (
            <option key={value} value={value}>
              {ticketPriorityLabels[value]}
            </option>
          ))}
        </select>
      </FormField>
      <FormError error={create.error} />
      <button type="submit" className="btn" disabled={create.isPending}>
        {create.isPending ? 'Kaydediliyor…' : 'Talep Oluştur'}
      </button>
    </form>
  );
}
