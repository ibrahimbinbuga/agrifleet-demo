import { useTelemetry } from '../telemetry/TelemetryProvider';
import type { AlertSeverity } from '../types';

const severityClasses: Record<AlertSeverity, string> = {
  Info: 'border-sky-400',
  Warning: 'border-amber-400',
  Critical: 'border-red-400',
};

export function ToastStack() {
  const { alerts, dismissAlert } = useTelemetry();

  return (
    <div className="pointer-events-none fixed right-4 top-16 z-[1000] w-80 max-w-[calc(100vw-2rem)] space-y-2">
      {alerts.map((alert) => (
        <div
          key={alert.id}
          role="status"
          className={`toast-enter pointer-events-auto flex items-start gap-3 border-l-2 bg-ops-800 px-3 py-2 text-sm shadow-lg ${severityClasses[alert.severity]}`}
        >
          <div className="min-w-0 flex-1">
            <p className="font-mono text-xs text-ops-300">{alert.serialNumber}</p>
            <p>{alert.message}</p>
          </div>
          <button
            type="button"
            aria-label="Uyarıyı kapat"
            onClick={() => dismissAlert(alert.id)}
            className="text-ops-500 hover:text-ops-100"
          >
            ×
          </button>
        </div>
      ))}
    </div>
  );
}
