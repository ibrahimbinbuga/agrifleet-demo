import { HubConnectionBuilder, LogLevel } from '@microsoft/signalr';
import { useQueryClient } from '@tanstack/react-query';
import { createContext, useCallback, useContext, useEffect, useMemo, useRef, useState, type ReactNode } from 'react';
import type { DroneAlert, DroneStatusChange, LiveTelemetry } from '../types';
import { applyBatch, applyStatusChange, emptyLiveState, type LiveState } from './liveState';

export type ConnectionStatus = 'connecting' | 'connected' | 'reconnecting';

export interface ToastAlert extends DroneAlert {
  id: number;
}

interface TelemetryContextValue extends LiveState {
  connection: ConnectionStatus;
  alerts: ToastAlert[];
  dismissAlert: (id: number) => void;
}

const MAX_RETRY_DELAY_MS = 30_000;
const START_RETRY_DELAY_MS = 3_000;
const ALERT_LIFETIME_MS = 8_000;
const MAX_VISIBLE_ALERTS = 4;

const retryForever = {
  nextRetryDelayInMilliseconds: (context: { previousRetryCount: number }) =>
    Math.min(1000 * 2 ** context.previousRetryCount, MAX_RETRY_DELAY_MS),
};

const TelemetryContext = createContext<TelemetryContextValue | null>(null);

export function TelemetryProvider({ children }: { children: ReactNode }) {
  const queryClient = useQueryClient();
  const nextAlertId = useRef(1);
  const [connection, setConnection] = useState<ConnectionStatus>('connecting');
  const [live, setLive] = useState<LiveState>(emptyLiveState);
  const [alerts, setAlerts] = useState<ToastAlert[]>([]);

  const dismissAlert = useCallback((id: number) => {
    setAlerts((current) => current.filter((alert) => alert.id !== id));
  }, []);

  useEffect(() => {
    const hub = new HubConnectionBuilder()
      .withUrl('/hubs/telemetry')
      .withAutomaticReconnect(retryForever)
      .configureLogging(LogLevel.Warning)
      .build();

    let retryTimer: ReturnType<typeof setTimeout> | undefined;
    let stopped = false;

    hub.on('TelemetryBatch', (batch: LiveTelemetry[]) => setLive((state) => applyBatch(state, batch)));

    hub.on('DroneStatusChanged', (change: DroneStatusChange) => {
      setLive((state) => applyStatusChange(state, change));
      void queryClient.invalidateQueries();
    });

    hub.on('AlertRaised', (alert: DroneAlert) => {
      const id = nextAlertId.current++;
      setAlerts((current) => [...current, { ...alert, id }].slice(-MAX_VISIBLE_ALERTS));
      setTimeout(() => dismissAlert(id), ALERT_LIFETIME_MS);
    });

    hub.onreconnecting(() => setConnection('reconnecting'));
    hub.onreconnected(() => {
      setConnection('connected');
      setLive(emptyLiveState);
      void queryClient.invalidateQueries();
    });

    const start = () => {
      hub.start().then(
        () => setConnection('connected'),
        () => {
          if (!stopped) {
            retryTimer = setTimeout(start, START_RETRY_DELAY_MS);
          }
        },
      );
    };

    // Deferred so React StrictMode's mount-unmount-mount cycle never starts a connection it immediately stops.
    retryTimer = setTimeout(start, 0);

    return () => {
      stopped = true;
      clearTimeout(retryTimer);
      void hub.stop();
    };
  }, [queryClient, dismissAlert]);

  const value = useMemo(
    () => ({ ...live, connection, alerts, dismissAlert }),
    [live, connection, alerts, dismissAlert],
  );

  return <TelemetryContext.Provider value={value}>{children}</TelemetryContext.Provider>;
}

export function useTelemetry(): TelemetryContextValue {
  const value = useContext(TelemetryContext);

  if (!value) {
    throw new Error('useTelemetry must be used inside TelemetryProvider');
  }

  return value;
}
