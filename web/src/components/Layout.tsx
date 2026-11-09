import { NavLink, Outlet } from 'react-router-dom';
import { useTelemetry, type ConnectionStatus } from '../telemetry/TelemetryProvider';
import { ToastStack } from './ToastStack';

const navigation = [
  { to: '/', label: 'Operasyon Merkezi', end: true },
  { to: '/drones', label: 'Filo', end: false },
  { to: '/maintenance', label: 'Bakım Planı', end: false },
  { to: '/tickets', label: 'Teknik Servis', end: false },
];

const connectionDisplay: Record<ConnectionStatus, { label: string; dot: string }> = {
  connecting: { label: 'Bağlanıyor', dot: 'bg-amber-400' },
  connected: { label: 'Canlı', dot: 'bg-field-500' },
  reconnecting: { label: 'Yeniden bağlanıyor', dot: 'bg-red-400' },
};

export function Layout() {
  const { connection } = useTelemetry();
  const display = connectionDisplay[connection];

  return (
    <div className="flex h-full flex-col">
      <header className="flex h-12 shrink-0 items-center gap-6 border-b border-ops-700 bg-ops-900 px-4">
        <span className="font-mono text-sm font-semibold tracking-wide text-field-400">AGRIFLEET</span>
        <nav className="flex min-w-0 flex-1 gap-1 overflow-x-auto">
          {navigation.map((item) => (
            <NavLink
              key={item.to}
              to={item.to}
              end={item.end}
              className={({ isActive }) =>
                `whitespace-nowrap px-3 py-1.5 text-sm ${isActive ? 'bg-ops-800 text-ops-100' : 'text-ops-300 hover:text-ops-100'}`
              }
            >
              {item.label}
            </NavLink>
          ))}
        </nav>
        <span className="flex shrink-0 items-center gap-2 text-xs text-ops-300" data-testid="connection">
          <span className={`h-2 w-2 rounded-full ${display.dot}`} />
          {display.label}
        </span>
      </header>
      <main className="min-h-0 flex-1 overflow-auto">
        <Outlet />
      </main>
      <ToastStack />
    </div>
  );
}
