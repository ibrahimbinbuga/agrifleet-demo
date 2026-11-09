import type { DroneStatusChange, LiveTelemetry } from '../types';

const TRAIL_LENGTH = 40;

export interface LiveState {
  positions: Record<string, LiveTelemetry>;
  trails: Record<string, [number, number][]>;
}

export const emptyLiveState: LiveState = { positions: {}, trails: {} };

export function applyBatch(state: LiveState, batch: LiveTelemetry[]): LiveState {
  const positions: LiveState['positions'] = {};
  const trails: LiveState['trails'] = {};

  for (const [droneId, position] of Object.entries(state.positions)) {
    if (position.phase === 'Charging') {
      positions[droneId] = position;
      trails[droneId] = state.trails[droneId] ?? [];
    }
  }

  for (const point of batch) {
    const previous = state.positions[point.droneId];
    const trail = previous?.flightId === point.flightId ? (state.trails[point.droneId] ?? []) : [];

    positions[point.droneId] = point;
    trails[point.droneId] = [...trail, [point.lat, point.lng] as [number, number]].slice(-TRAIL_LENGTH);
  }

  return { positions, trails };
}

export function applyStatusChange(state: LiveState, change: DroneStatusChange): LiveState {
  const position = state.positions[change.droneId];

  if (!position || change.status === 'InFlight') {
    return state;
  }

  if (change.status === 'Charging') {
    return {
      ...state,
      positions: { ...state.positions, [change.droneId]: { ...position, phase: 'Charging', speedMs: 0, altitudeM: 0 } },
    };
  }

  const { [change.droneId]: _removedPosition, ...positions } = state.positions;
  const { [change.droneId]: _removedTrail, ...trails } = state.trails;

  return { positions, trails };
}
