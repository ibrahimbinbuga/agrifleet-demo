import { CartesianGrid, Line, LineChart, ResponsiveContainer, Tooltip, XAxis, YAxis } from 'recharts';
import { formatNumber, formatTime } from '../lib/format';
import type { TelemetryPoint } from '../types';

const AXIS_COLOR = '#a9b8af';
const GRID_COLOR = '#2a372f';
const LINE_COLOR = '#7fd18a';

interface ChartPoint {
  time: string;
  value: number;
}

function MetricChart({ title, unit, data, domain }: {
  title: string;
  unit: string;
  data: ChartPoint[];
  domain: [number, number];
}) {
  return (
    <figure>
      <figcaption className="px-3 py-2 text-[11px] uppercase tracking-wide text-ops-500">
        {title} ({unit})
      </figcaption>
      <ResponsiveContainer width="100%" height={150}>
        <LineChart data={data} margin={{ top: 4, right: 16, bottom: 0, left: 0 }}>
          <CartesianGrid vertical={false} stroke={GRID_COLOR} />
          <XAxis dataKey="time" minTickGap={56} tick={{ fill: AXIS_COLOR, fontSize: 11 }} tickLine={false} axisLine={{ stroke: GRID_COLOR }} />
          <YAxis domain={domain} width={36} tick={{ fill: AXIS_COLOR, fontSize: 11 }} tickLine={false} axisLine={false} />
          <Tooltip
            cursor={{ stroke: AXIS_COLOR, strokeWidth: 1 }}
            contentStyle={{ background: '#1b251f', border: `1px solid ${GRID_COLOR}`, fontSize: 12 }}
            labelStyle={{ color: AXIS_COLOR }}
            itemStyle={{ color: '#e4ebe6' }}
            formatter={(value) => [`${formatNumber(Number(value), 1)} ${unit}`, title]}
          />
          <Line type="monotone" dataKey="value" stroke={LINE_COLOR} strokeWidth={2} dot={false} isAnimationActive={false} />
        </LineChart>
      </ResponsiveContainer>
    </figure>
  );
}

export function FlightCharts({ telemetry }: { telemetry: TelemetryPoint[] }) {
  const series = (select: (point: TelemetryPoint) => number): ChartPoint[] =>
    telemetry.map((point) => ({ time: formatTime(point.timestamp), value: select(point) }));
  const maxAltitude = Math.ceil(Math.max(...telemetry.map((point) => point.altitudeM), 1));

  return (
    <div>
      <MetricChart title="Batarya" unit="%" data={series((point) => point.batteryPct)} domain={[0, 100]} />
      <MetricChart title="İrtifa" unit="m" data={series((point) => point.altitudeM)} domain={[0, maxAltitude]} />
    </div>
  );
}
