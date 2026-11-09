import 'package:flutter/material.dart';

import '../data/fleet_repository.dart';
import '../data/models.dart';
import '../labels.dart';
import '../widgets/add_maintenance_sheet.dart';
import '../widgets/maintenance_badge.dart';
import '../widgets/status_views.dart';
import 'create_ticket_screen.dart';

typedef _DroneDetail = ({Drone drone, List<MaintenanceRecord> history});

class DroneDetailScreen extends StatefulWidget {
  const DroneDetailScreen({
    super.key,
    required this.repository,
    required this.droneId,
    required this.serialNumber,
  });

  final FleetRepository repository;
  final String droneId;
  final String serialNumber;

  @override
  State<DroneDetailScreen> createState() => _DroneDetailScreenState();
}

class _DroneDetailScreenState extends State<DroneDetailScreen> {
  late Future<_DroneDetail> _detail = _fetch();

  Future<_DroneDetail> _fetch() async {
    final drone = widget.repository.getDrone(widget.droneId);
    final history = widget.repository.getMaintenanceHistory(widget.droneId);

    return (drone: await drone, history: await history);
  }

  void _reload() {
    setState(() {
      _detail = _fetch();
    });
  }

  void _showMessage(String message) {
    ScaffoldMessenger.of(
      context,
    ).showSnackBar(SnackBar(content: Text(message)));
  }

  Future<void> _addMaintenance() async {
    final saved = await showModalBottomSheet<bool>(
      context: context,
      isScrollControlled: true,
      builder: (_) => AddMaintenanceSheet(
        repository: widget.repository,
        droneId: widget.droneId,
      ),
    );

    if (saved == true && mounted) {
      _showMessage('Bakım kaydı eklendi.');
      _reload();
    }
  }

  Future<void> _createTicket() async {
    final created = await Navigator.of(context).push<bool>(
      MaterialPageRoute(
        builder: (_) => CreateTicketScreen(
          repository: widget.repository,
          initialDroneId: widget.droneId,
        ),
      ),
    );

    if (created == true && mounted) {
      _showMessage('Servis talebi oluşturuldu.');
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(title: Text(widget.serialNumber)),
      body: FutureBuilder<_DroneDetail>(
        future: _detail,
        builder: (context, snapshot) {
          if (snapshot.connectionState != ConnectionState.done) {
            return const LoadingView();
          }
          if (snapshot.hasError) {
            return ErrorView(error: snapshot.error, onRetry: _reload);
          }

          final (:drone, :history) = snapshot.requireData;

          return ListView(
            padding: const EdgeInsets.all(16),
            children: [
              Row(
                children: [
                  Expanded(
                    child: Text(
                      droneStatusLabels[drone.status] ?? drone.status,
                      style: Theme.of(context).textTheme.titleMedium,
                    ),
                  ),
                  MaintenanceBadge(status: drone.maintenanceStatus),
                ],
              ),
              const SizedBox(height: 4),
              Text(drone.maintenanceReason),
              const SizedBox(height: 16),
              _InfoRow('Model', drone.model),
              _InfoRow(
                'Tank',
                '${drone.tankCapacityLiters.toStringAsFixed(0)} L',
              ),
              _InfoRow('Toplam Uçuş', formatHours(drone.totalFlightMinutes)),
              _InfoRow(
                'Bakımdan Beri',
                formatHours(drone.flightMinutesSinceMaintenance),
              ),
              _InfoRow('Batarya Döngüsü', '${drone.batteryCycles}'),
              _InfoRow('Son Bakım', formatDateTime(drone.lastMaintenanceAt)),
              const SizedBox(height: 16),
              FilledButton.icon(
                onPressed: drone.isFlying ? null : _addMaintenance,
                icon: const Icon(Icons.add),
                label: const Text('Bakım Kaydı Ekle'),
              ),
              if (drone.isFlying)
                const Padding(
                  padding: EdgeInsets.only(top: 6),
                  child: Text(
                    'Uçuştaki İHA için bakım kaydı eklenemez.',
                    textAlign: TextAlign.center,
                  ),
                ),
              const SizedBox(height: 8),
              OutlinedButton.icon(
                onPressed: _createTicket,
                icon: const Icon(Icons.build_outlined),
                label: const Text('Servis Talebi Oluştur'),
              ),
              const SizedBox(height: 24),
              Text(
                'Bakım Geçmişi',
                style: Theme.of(context).textTheme.titleMedium,
              ),
              const SizedBox(height: 4),
              if (history.isEmpty)
                const Padding(
                  padding: EdgeInsets.symmetric(vertical: 12),
                  child: Text('Bakım kaydı yok.'),
                ),
              for (final record in history)
                ListTile(
                  contentPadding: EdgeInsets.zero,
                  title: Text(record.description),
                  subtitle: Text(
                    '${maintenanceTypeLabels[record.type] ?? record.type} · '
                    '${record.performedBy} · ${formatDateTime(record.performedAt)}',
                  ),
                ),
            ],
          );
        },
      ),
    );
  }
}

class _InfoRow extends StatelessWidget {
  const _InfoRow(this.label, this.value);

  final String label;
  final String value;

  @override
  Widget build(BuildContext context) {
    return Padding(
      padding: const EdgeInsets.symmetric(vertical: 4),
      child: Row(
        mainAxisAlignment: MainAxisAlignment.spaceBetween,
        children: [Text(label), Text(value)],
      ),
    );
  }
}
