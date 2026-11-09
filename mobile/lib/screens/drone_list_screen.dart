import 'package:flutter/material.dart';

import '../data/fleet_repository.dart';
import '../data/models.dart';
import '../labels.dart';
import '../widgets/maintenance_badge.dart';
import '../widgets/status_views.dart';
import 'create_ticket_screen.dart';
import 'drone_detail_screen.dart';

class DroneListScreen extends StatefulWidget {
  const DroneListScreen({super.key, required this.repository});

  final FleetRepository repository;

  @override
  State<DroneListScreen> createState() => _DroneListScreenState();
}

class _DroneListScreenState extends State<DroneListScreen> {
  late Future<List<Drone>> _drones = widget.repository.getDrones();

  void _reload() {
    setState(() {
      _drones = widget.repository.getDrones();
    });
  }

  Future<void> _refresh() async {
    _reload();
    await _drones.then((_) {}, onError: (_) {});
  }

  Future<void> _openDrone(Drone drone) async {
    await Navigator.of(context).push(
      MaterialPageRoute<void>(
        builder: (_) => DroneDetailScreen(
          repository: widget.repository,
          droneId: drone.id,
          serialNumber: drone.serialNumber,
        ),
      ),
    );
    if (mounted) _reload();
  }

  Future<void> _createTicket() async {
    final created = await Navigator.of(context).push<bool>(
      MaterialPageRoute(
        builder: (_) => CreateTicketScreen(repository: widget.repository),
      ),
    );

    if (created == true && mounted) {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('Servis talebi oluşturuldu.')),
      );
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(title: const Text('İHA Filosu')),
      floatingActionButton: FloatingActionButton.extended(
        onPressed: _createTicket,
        icon: const Icon(Icons.build_outlined),
        label: const Text('Servis Talebi'),
      ),
      body: FutureBuilder<List<Drone>>(
        future: _drones,
        builder: (context, snapshot) {
          if (snapshot.connectionState != ConnectionState.done) {
            return const LoadingView();
          }
          if (snapshot.hasError) {
            return ErrorView(error: snapshot.error, onRetry: _reload);
          }

          final drones = snapshot.requireData;

          if (drones.isEmpty) {
            return const Center(child: Text('Kayıtlı İHA yok.'));
          }

          return RefreshIndicator(
            onRefresh: _refresh,
            child: ListView.separated(
              padding: const EdgeInsets.only(bottom: 88),
              itemCount: drones.length,
              separatorBuilder: (_, _) => const Divider(height: 1),
              itemBuilder: (context, index) {
                final drone = drones[index];

                return ListTile(
                  title: Text(drone.serialNumber),
                  subtitle: Text(
                    '${drone.model} · ${droneStatusLabels[drone.status] ?? drone.status}',
                  ),
                  trailing: MaintenanceBadge(status: drone.maintenanceStatus),
                  onTap: () => _openDrone(drone),
                );
              },
            ),
          );
        },
      ),
    );
  }
}
