import 'package:flutter/material.dart';

import '../labels.dart';

class MaintenanceBadge extends StatelessWidget {
  const MaintenanceBadge({super.key, required this.status});

  final String status;

  @override
  Widget build(BuildContext context) {
    final color = switch (status) {
      'Overdue' => Colors.red,
      'DueSoon' => Colors.amber,
      _ => Colors.green,
    };
    final isDark = Theme.of(context).brightness == Brightness.dark;
    final lightText = status == 'DueSoon'
        ? Colors.brown.shade800
        : color.shade900;

    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 3),
      decoration: BoxDecoration(
        color: color.withValues(alpha: 0.18),
        borderRadius: BorderRadius.circular(4),
      ),
      child: Text(
        maintenanceStatusLabels[status] ?? status,
        style: TextStyle(
          color: isDark ? color.shade200 : lightText,
          fontSize: 12,
          fontWeight: FontWeight.w600,
        ),
      ),
    );
  }
}
