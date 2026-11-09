import { Link } from 'react-router-dom';
import { formatNumber } from '../lib/format';
import { phaseLabels } from '../lib/labels';
import { useTelemetry } from '../telemetry/TelemetryProvider';
import { BarMeter, EmptyState } from './ui';

export function LiveTelemetryPanel() {
  const { positions } = useTelemetry();
  const airborne = Object.values(positions)
    .filter((point) => point.phase !== 'Charging')
    .sort((a, b) => a.serialNumber.localeCompare(b.serialNumber));

  return (
    <section aria-label="Canlı telemetri" className="flex h-full flex-col">
      <h2 className="border-b border-ops-700 px-4 py-2 text-[11px] uppercase tracking-wide text-ops-500">
        Canlı Telemetri · {airborne.length} İHA
      </h2>

      {airborne.length === 0 ? (
        <EmptyState text="Şu an uçuşta İHA yok." />
      ) : (
        <ul className="min-h-0 flex-1 divide-y divide-ops-700 overflow-auto" data-testid="live-panel">
          {airborne.map((point) => (
            <li key={point.droneId} className="space-y-2 px-4 py-3">
              <div className="flex items-baseline justify-between gap-2">
                <Link to={`/drones/${point.droneId}`} className="font-mono text-sm text-field-400 hover:underline">
                  {point.serialNumber}
                </Link>
                <span className="text-xs text-ops-300">{phaseLabels[point.phase]}</span>
              </div>
              <BarMeter label="Batarya" value={point.batteryPct} warnBelow={20} />
              <BarMeter label="Tank" value={point.tankLevelPct} warnBelow={10} />
              <div className="flex gap-4 font-mono text-xs tabular-nums text-ops-300">
                <span>{formatNumber(point.altitudeM, 1)} m</span>
                <span>{formatNumber(point.speedMs, 1)} m/sn</span>
              </div>
            </li>
          ))}
        </ul>
      )}
    </section>
  );
}
