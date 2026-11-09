import { useQuery } from '@tanstack/react-query';
import { CircleMarker, MapContainer, Polygon, Polyline, TileLayer, Tooltip } from 'react-leaflet';
import { api } from '../api';
import { useTelemetry } from '../telemetry/TelemetryProvider';
import type { LiveTelemetry } from '../types';
import { ErrorState, LoadingState } from './ui';

const LOW_BATTERY_PCT = 20;

const legend = [
  { label: 'İlaçlama', color: '#7fd18a' },
  { label: 'Seyir', color: '#e4c76b' },
  { label: 'Düşük batarya', color: '#f87171' },
  { label: 'Şarjda', color: '#7dd3fc' },
];

function markerColor(point: LiveTelemetry): string {
  if (point.phase === 'Charging') return '#7dd3fc';
  if (point.batteryPct < LOW_BATTERY_PCT) return '#f87171';
  return point.phase === 'Spraying' ? '#7fd18a' : '#e4c76b';
}

export function LiveMap() {
  const { positions, trails } = useTelemetry();
  const { data: fields, isPending, error } = useQuery({ queryKey: ['fields'], queryFn: api.getFields });

  if (isPending) return <LoadingState text="Harita yükleniyor…" />;
  if (error) return <ErrorState error={error} />;

  return (
    <div className="relative h-full min-h-[24rem]">
      <MapContainer
        bounds={fields.flatMap((field) => field.boundary)}
        boundsOptions={{ padding: [32, 32] }}
        className="h-full w-full"
      >
        <TileLayer
          url="https://tile.openstreetmap.org/{z}/{x}/{y}.png"
          attribution='&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a>'
        />

        {fields.map((field) => (
          <Polygon
            key={field.id}
            positions={field.boundary}
            pathOptions={{ color: '#57b865', weight: 1.5, fillOpacity: 0.14 }}
          >
            <Tooltip sticky className="map-label">
              {field.name} · {field.cropType} · {field.areaHectares} ha
            </Tooltip>
          </Polygon>
        ))}

        {Object.values(positions).map((point) => {
          const color = markerColor(point);

          return (
            <CircleMarker
              key={point.droneId}
              center={[point.lat, point.lng]}
              radius={6}
              pathOptions={{ color: '#0a0f0c', weight: 2, fillColor: color, fillOpacity: 1 }}
            >
              <Tooltip permanent direction="top" offset={[0, -6]} className="map-label">
                {point.serialNumber}
              </Tooltip>
            </CircleMarker>
          );
        })}

        {Object.entries(trails).map(([droneId, trail]) => {
          const point = positions[droneId];

          return point ? (
            <Polyline
              key={droneId}
              positions={trail}
              interactive={false}
              pathOptions={{ color: markerColor(point), weight: 2, opacity: 0.7 }}
            />
          ) : null;
        })}
      </MapContainer>

      <ul className="absolute bottom-6 left-3 z-[500] space-y-1 bg-ops-950/85 px-2.5 py-2 text-[11px] text-ops-300">
        {legend.map((item) => (
          <li key={item.label} className="flex items-center gap-2">
            <span className="h-2 w-2 rounded-full" style={{ backgroundColor: item.color }} />
            {item.label}
          </li>
        ))}
      </ul>
    </div>
  );
}
