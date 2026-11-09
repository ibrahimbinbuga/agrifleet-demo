import { Link, Route, Routes } from 'react-router-dom';
import { Layout } from './components/Layout';
import { DroneDetailPage } from './pages/DroneDetailPage';
import { DronesPage } from './pages/DronesPage';
import { FlightDetailPage } from './pages/FlightDetailPage';
import { MaintenancePage } from './pages/MaintenancePage';
import { OperationsPage } from './pages/OperationsPage';
import { TicketsPage } from './pages/TicketsPage';

function NotFoundPage() {
  return (
    <p className="p-6 text-sm text-ops-300">
      Sayfa bulunamadı.{' '}
      <Link className="text-field-400 hover:underline" to="/">
        Operasyon Merkezi'ne dön
      </Link>
    </p>
  );
}

export default function App() {
  return (
    <Routes>
      <Route element={<Layout />}>
        <Route index element={<OperationsPage />} />
        <Route path="drones" element={<DronesPage />} />
        <Route path="drones/:id" element={<DroneDetailPage />} />
        <Route path="flights/:id" element={<FlightDetailPage />} />
        <Route path="maintenance" element={<MaintenancePage />} />
        <Route path="tickets" element={<TicketsPage />} />
        <Route path="*" element={<NotFoundPage />} />
      </Route>
    </Routes>
  );
}
