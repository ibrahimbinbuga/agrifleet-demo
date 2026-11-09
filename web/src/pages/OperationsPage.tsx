import { KpiStrip } from '../components/KpiStrip';
import { LiveMap } from '../components/LiveMap';
import { LiveTelemetryPanel } from '../components/LiveTelemetryPanel';

export function OperationsPage() {
  return (
    <div className="flex h-full flex-col">
      <KpiStrip />
      <div className="grid min-h-0 flex-1 grid-cols-1 lg:grid-cols-[minmax(0,1fr)_20rem]">
        <LiveMap />
        <div className="min-h-0 border-t border-ops-700 bg-ops-900 lg:border-l lg:border-t-0">
          <LiveTelemetryPanel />
        </div>
      </div>
    </div>
  );
}
