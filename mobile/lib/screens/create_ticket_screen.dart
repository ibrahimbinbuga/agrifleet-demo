import 'package:flutter/material.dart';

import '../data/fleet_repository.dart';
import '../data/models.dart';
import '../labels.dart';
import '../widgets/status_views.dart';

class CreateTicketScreen extends StatefulWidget {
  const CreateTicketScreen({
    super.key,
    required this.repository,
    this.initialDroneId,
  });

  final FleetRepository repository;
  final String? initialDroneId;

  @override
  State<CreateTicketScreen> createState() => _CreateTicketScreenState();
}

class _CreateTicketScreenState extends State<CreateTicketScreen> {
  final _formKey = GlobalKey<FormState>();
  final _title = TextEditingController();
  final _description = TextEditingController();
  late Future<List<Drone>> _drones = widget.repository.getDrones();
  late String? _droneId = widget.initialDroneId;
  String _priority = 'Medium';
  bool _saving = false;
  ApiException? _error;

  @override
  void dispose() {
    _title.dispose();
    _description.dispose();
    super.dispose();
  }

  Future<void> _submit() async {
    if (!_formKey.currentState!.validate()) return;

    setState(() {
      _saving = true;
      _error = null;
    });

    try {
      await widget.repository.createTicket(
        droneId: _droneId!,
        title: _title.text,
        description: _description.text,
        priority: _priority,
      );
      if (mounted) Navigator.of(context).pop(true);
    } on ApiException catch (error) {
      if (mounted) {
        setState(() {
          _error = error;
          _saving = false;
        });
      }
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(title: const Text('Servis Talebi')),
      body: FutureBuilder<List<Drone>>(
        future: _drones,
        builder: (context, snapshot) {
          if (snapshot.connectionState != ConnectionState.done) {
            return const LoadingView();
          }
          if (snapshot.hasError) {
            return ErrorView(
              error: snapshot.error,
              onRetry: () {
                setState(() {
                  _drones = widget.repository.getDrones();
                });
              },
            );
          }

          return _buildForm(snapshot.requireData);
        },
      ),
    );
  }

  Widget _buildForm(List<Drone> drones) {
    final error = _error;

    return Form(
      key: _formKey,
      child: ListView(
        padding: const EdgeInsets.all(16),
        children: [
          DropdownButtonFormField<String>(
            initialValue: _droneId,
            decoration: const InputDecoration(labelText: 'İHA'),
            items: [
              for (final drone in drones)
                DropdownMenuItem(
                  value: drone.id,
                  child: Text(drone.serialNumber),
                ),
            ],
            validator: (value) => value == null ? 'İHA seçilmelidir.' : null,
            onChanged: (value) => setState(() => _droneId = value),
          ),
          const SizedBox(height: 12),
          TextField(
            controller: _title,
            decoration: InputDecoration(
              labelText: 'Başlık',
              errorText: error?.fieldError('Title'),
            ),
          ),
          const SizedBox(height: 12),
          TextField(
            controller: _description,
            maxLines: 3,
            decoration: InputDecoration(
              labelText: 'Açıklama',
              errorText: error?.fieldError('Description'),
            ),
          ),
          const SizedBox(height: 12),
          DropdownButtonFormField<String>(
            initialValue: _priority,
            decoration: const InputDecoration(labelText: 'Öncelik'),
            items: [
              for (final entry in ticketPriorityLabels.entries)
                DropdownMenuItem(value: entry.key, child: Text(entry.value)),
            ],
            onChanged: (value) =>
                setState(() => _priority = value ?? _priority),
          ),
          if (error != null && !error.hasFieldErrors)
            Padding(
              padding: const EdgeInsets.only(top: 12),
              child: Text(
                error.message,
                style: TextStyle(color: Theme.of(context).colorScheme.error),
              ),
            ),
          const SizedBox(height: 16),
          FilledButton(
            onPressed: _saving ? null : _submit,
            child: Text(_saving ? 'Kaydediliyor…' : 'Talep Oluştur'),
          ),
        ],
      ),
    );
  }
}
