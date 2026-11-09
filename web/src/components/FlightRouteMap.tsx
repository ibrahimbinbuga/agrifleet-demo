import { CircleMarker, MapContainer, Polygon, Polyline, TileLayer, Tooltip } from 'react-leaflet';
import type { Field, TelemetryPoint } from '../types';

export function FlightRouteMap({ telemetry, field }: { telemetry: TelemetryPoint[]; field?: Field }) {
  const route = telemetry.map((point): [number, number] => [point.lat, point.lng]);
  const start = route[0];
  const end = route[route.length - 1];

  return (
    <MapContainer bounds={route} boundsOptions={{ padding: [28, 28] }} className="h-full min-h-[22rem] w-full">
      <TileLayer
        url="https://tile.openstreetmap.org/{z}/{x}/{y}.png"
        attribution='&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a>'
      />
      {field && <Polygon positions={field.boundary} pathOptions={{ color: '#57b865', weight: 1.5, fillOpacity: 0.12 }} />}
      <Polyline positions={route} pathOptions={{ color: '#7fd18a', weight: 2 }} />
      {start && (
        <CircleMarker center={start} radius={6} pathOptions={{ color: '#0a0f0c', weight: 2, fillColor: '#e4c76b', fillOpacity: 1 }}>
          <Tooltip permanent direction="top" offset={[0, -6]} className="map-label">
            Başlangıç
          </Tooltip>
        </CircleMarker>
      )}
      {end && (
        <CircleMarker center={end} radius={6} pathOptions={{ color: '#0a0f0c', weight: 2, fillColor: '#7dd3fc', fillOpacity: 1 }}>
          <Tooltip permanent direction="bottom" offset={[0, 6]} className="map-label">
            Bitiş
          </Tooltip>
        </CircleMarker>
      )}
    </MapContainer>
  );
}
