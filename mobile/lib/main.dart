import 'package:flutter/material.dart';

import 'data/fleet_repository.dart';
import 'screens/drone_list_screen.dart';

const _seedColor = Color(0xFF3F9A4F);

void main() => runApp(TechnicianApp(repository: FleetRepository()));

class TechnicianApp extends StatelessWidget {
  const TechnicianApp({super.key, required this.repository});

  final FleetRepository repository;

  @override
  Widget build(BuildContext context) {
    return MaterialApp(
      title: 'AgriFleet Teknisyen',
      debugShowCheckedModeBanner: false,
      themeMode: ThemeMode.system,
      theme: ThemeData(colorSchemeSeed: _seedColor),
      darkTheme: ThemeData(
        colorSchemeSeed: _seedColor,
        brightness: Brightness.dark,
      ),
      home: DroneListScreen(repository: repository),
    );
  }
}
