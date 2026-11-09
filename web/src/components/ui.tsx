import type { ReactNode } from 'react';
import { ApiError } from '../api';
import type { Tone } from '../lib/labels';

export function PageHeader({ title, subtitle, children }: { title: string; subtitle?: string; children?: ReactNode }) {
  return (
    <header className="flex flex-wrap items-end justify-between gap-3 border-b border-ops-700 px-6 py-4">
      <div>
        <h1 className="text-lg font-semibold">{title}</h1>
        {subtitle && <p className="text-sm text-ops-500">{subtitle}</p>}
      </div>
      {children}
    </header>
  );
}

export function SectionTitle({ children }: { children: ReactNode }) {
  return <h2 className="px-3 py-2 text-[11px] uppercase tracking-wide text-ops-500">{children}</h2>;
}

export function FormField({ label, error, children }: { label: string; error?: string; children: ReactNode }) {
  return (
    <label className="block text-xs text-ops-300">
      {label}
      <div className="mt-1">{children}</div>
      {error && <span className="mt-1 block text-red-300">{error}</span>}
    </label>
  );
}

export function fieldError(error: unknown, field: string): string | undefined {
  return error instanceof ApiError ? error.fieldErrors[field]?.[0] : undefined;
}

export function FormError({ error }: { error: unknown }) {
  if (!error || (error instanceof ApiError && Object.keys(error.fieldErrors).length > 0)) {
    return null;
  }

  return (
    <p role="alert" className="text-xs text-red-300">
      {error instanceof Error ? error.message : 'Beklenmeyen bir hata oluştu.'}
    </p>
  );
}

export function Pager({ page, pageSize, totalCount, onPageChange }: {
  page: number;
  pageSize: number;
  totalCount: number;
  onPageChange: (page: number) => void;
}) {
  const lastPage = Math.max(1, Math.ceil(totalCount / pageSize));

  return (
    <div className="flex items-center justify-end gap-3 px-3 py-2 text-xs text-ops-500">
      <span>
        Sayfa {page} / {lastPage} · {totalCount} kayıt
      </span>
      <button type="button" className="btn-quiet" disabled={page <= 1} onClick={() => onPageChange(page - 1)}>
        Önceki
      </button>
      <button type="button" className="btn-quiet" disabled={page >= lastPage} onClick={() => onPageChange(page + 1)}>
        Sonraki
      </button>
    </div>
  );
}

const toneClasses: Record<Tone, string> = {
  green: 'bg-field-600/20 text-field-400',
  amber: 'bg-amber-500/15 text-amber-300',
  red: 'bg-red-500/15 text-red-300',
  blue: 'bg-sky-500/15 text-sky-300',
  neutral: 'bg-ops-700/60 text-ops-300',
};

export function Badge({ tone, children }: { tone: Tone; children: ReactNode }) {
  return (
    <span className={`inline-block whitespace-nowrap rounded-sm px-1.5 py-0.5 text-xs font-medium ${toneClasses[tone]}`}>
      {children}
    </span>
  );
}

export function LoadingState({ text = 'Yükleniyor…' }: { text?: string }) {
  return <p className="p-6 text-sm text-ops-500">{text}</p>;
}

export function EmptyState({ text }: { text: string }) {
  return <p className="p-6 text-sm text-ops-500">{text}</p>;
}

export function ErrorState({ error }: { error: unknown }) {
  const message = error instanceof Error ? error.message : 'Beklenmeyen bir hata oluştu.';

  return (
    <p role="alert" className="p-6 text-sm text-red-300">
      Veri alınamadı: {message}
    </p>
  );
}

export function BarMeter({ label, value, warnBelow }: { label: string; value: number; warnBelow: number }) {
  const barColor = value < warnBelow ? 'bg-red-400' : value < warnBelow * 2 ? 'bg-amber-400' : 'bg-field-500';

  return (
    <div>
      <div className="flex justify-between text-[11px] text-ops-500">
        <span>{label}</span>
        <span className="font-mono tabular-nums text-ops-100">%{Math.round(value)}</span>
      </div>
      <div
        className="mt-0.5 h-1 bg-ops-700"
        role="progressbar"
        aria-label={label}
        aria-valuemin={0}
        aria-valuemax={100}
        aria-valuenow={Math.round(value)}
      >
        <div className={`h-full transition-[width] duration-1000 ease-linear ${barColor}`} style={{ width: `${value}%` }} />
      </div>
    </div>
  );
}
