import 'package:flutter/material.dart';

import '../data/fleet_repository.dart';
import '../data/models.dart';
import '../labels.dart';

class AddMaintenanceSheet extends StatefulWidget {
  const AddMaintenanceSheet({
    super.key,
    required this.repository,
    required this.droneId,
  });

  final FleetRepository repository;
  final String droneId;

  @override
  State<AddMaintenanceSheet> createState() => _AddMaintenanceSheetState();
}

class _AddMaintenanceSheetState extends State<AddMaintenanceSheet> {
  final _description = TextEditingController();
  final _performedBy = TextEditingController();
  String _type = 'Scheduled';
  bool _saving = false;
  ApiException? _error;

  @override
  void dispose() {
    _description.dispose();
    _performedBy.dispose();
    super.dispose();
  }

  Future<void> _save() async {
    setState(() {
      _saving = true;
      _error = null;
    });

    try {
      await widget.repository.createMaintenance(
        droneId: widget.droneId,
        type: _type,
        description: _description.text,
        performedBy: _performedBy.text,
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
    final error = _error;

    return Padding(
      padding: EdgeInsets.fromLTRB(
        16,
        16,
        16,
        16 + MediaQuery.viewInsetsOf(context).bottom,
      ),
      child: SingleChildScrollView(
        child: Column(
          mainAxisSize: MainAxisSize.min,
          crossAxisAlignment: CrossAxisAlignment.stretch,
          children: [
            Text(
              'Bakım Kaydı Ekle',
              style: Theme.of(context).textTheme.titleLarge,
            ),
            const SizedBox(height: 16),
            DropdownButtonFormField<String>(
              initialValue: _type,
              decoration: const InputDecoration(labelText: 'Tip'),
              items: [
                for (final entry in maintenanceTypeLabels.entries)
                  DropdownMenuItem(value: entry.key, child: Text(entry.value)),
              ],
              onChanged: (value) => setState(() => _type = value ?? _type),
            ),
            const SizedBox(height: 12),
            TextField(
              controller: _description,
              maxLines: 2,
              decoration: InputDecoration(
                labelText: 'Açıklama',
                errorText: error?.fieldError('Description'),
              ),
            ),
            const SizedBox(height: 12),
            TextField(
              controller: _performedBy,
              decoration: InputDecoration(
                labelText: 'Yapan',
                errorText: error?.fieldError('PerformedBy'),
              ),
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
              onPressed: _saving ? null : _save,
              child: Text(_saving ? 'Kaydediliyor…' : 'Kaydet'),
            ),
          ],
        ),
      ),
    );
  }
}
