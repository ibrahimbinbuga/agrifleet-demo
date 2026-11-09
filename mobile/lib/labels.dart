const droneStatusLabels = {
  'Idle': 'Hazır',
  'InFlight': 'Uçuşta',
  'Charging': 'Şarjda',
  'Maintenance': 'Bakımda',
  'Offline': 'Çevrimdışı',
};

const maintenanceStatusLabels = {
  'Ok': 'Uygun',
  'DueSoon': 'Yaklaşıyor',
  'Overdue': 'Gecikmiş',
};

const maintenanceTypeLabels = {
  'Scheduled': 'Periyodik',
  'Repair': 'Onarım',
  'Inspection': 'Muayene',
};

const ticketPriorityLabels = {
  'Low': 'Düşük',
  'Medium': 'Orta',
  'High': 'Yüksek',
  'Critical': 'Kritik',
};

String formatDateTime(DateTime value) {
  final local = value.toLocal();
  String twoDigits(int number) => number.toString().padLeft(2, '0');

  return '${twoDigits(local.day)}.${twoDigits(local.month)}.${local.year} '
      '${twoDigits(local.hour)}:${twoDigits(local.minute)}';
}

String formatHours(double minutes) => '${(minutes / 60).toStringAsFixed(1)} sa';
