class Drone {
  const Drone({
    required this.id,
    required this.serialNumber,
    required this.model,
    required this.status,
    required this.tankCapacityLiters,
    required this.totalFlightMinutes,
    required this.flightMinutesSinceMaintenance,
    required this.batteryCycles,
    required this.lastMaintenanceAt,
    required this.maintenanceStatus,
    required this.maintenanceReason,
  });

  factory Drone.fromJson(Map<String, dynamic> json) => Drone(
    id: json['id'] as String,
    serialNumber: json['serialNumber'] as String,
    model: json['model'] as String,
    status: json['status'] as String,
    tankCapacityLiters: (json['tankCapacityLiters'] as num).toDouble(),
    totalFlightMinutes: (json['totalFlightMinutes'] as num).toDouble(),
    flightMinutesSinceMaintenance:
        (json['flightMinutesSinceMaintenance'] as num).toDouble(),
    batteryCycles: json['batteryCycles'] as int,
    lastMaintenanceAt: DateTime.parse(json['lastMaintenanceAt'] as String),
    maintenanceStatus: json['maintenanceStatus'] as String,
    maintenanceReason: json['maintenanceReason'] as String,
  );

  final String id;
  final String serialNumber;
  final String model;
  final String status;
  final double tankCapacityLiters;
  final double totalFlightMinutes;
  final double flightMinutesSinceMaintenance;
  final int batteryCycles;
  final DateTime lastMaintenanceAt;
  final String maintenanceStatus;
  final String maintenanceReason;

  bool get isFlying => status == 'InFlight';
}

class MaintenanceRecord {
  const MaintenanceRecord({
    required this.id,
    required this.type,
    required this.description,
    required this.performedBy,
    required this.performedAt,
  });

  factory MaintenanceRecord.fromJson(Map<String, dynamic> json) =>
      MaintenanceRecord(
        id: json['id'] as String,
        type: json['type'] as String,
        description: json['description'] as String,
        performedBy: json['performedBy'] as String,
        performedAt: DateTime.parse(json['performedAt'] as String),
      );

  final String id;
  final String type;
  final String description;
  final String performedBy;
  final DateTime performedAt;
}

class ApiException implements Exception {
  const ApiException(this.message, {this.fieldErrors = const {}});

  final String message;
  final Map<String, List<String>> fieldErrors;

  String? fieldError(String field) => fieldErrors[field]?.first;

  bool get hasFieldErrors => fieldErrors.isNotEmpty;

  @override
  String toString() => message;
}
