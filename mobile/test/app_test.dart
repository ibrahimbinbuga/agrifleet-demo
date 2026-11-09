import 'dart:convert';

import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:http/http.dart' as http;
import 'package:http/testing.dart';
import 'package:technician/data/fleet_repository.dart';
import 'package:technician/screens/create_ticket_screen.dart';
import 'package:technician/screens/drone_list_screen.dart';

const _droneId = '11111111-1111-1111-1111-111111111111';

Map<String, Object?> _droneJson(String serial, String maintenanceStatus) => {
  'id': _droneId,
  'serialNumber': serial,
  'model': 'AG-10',
  'status': 'Idle',
  'tankCapacityLiters': 10,
  'totalFlightMinutes': 6480,
  'flightMinutesSinceMaintenance': 3120.5,
  'batteryCycles': 655,
  'lastMaintenanceAt': '2026-08-31T17:09:23Z',
  'maintenanceStatus': maintenanceStatus,
  'maintenanceReason': 'Uçuş süresi limiti aşıldı',
};

http.Response _json(Object? body, [int status = 200]) =>
    http.Response.bytes(utf8.encode(jsonEncode(body)), status);

FleetRepository _repository(
  Future<http.Response> Function(http.Request request) handler,
) => FleetRepository(client: MockClient(handler), baseUrl: 'http://test');

Future<void> _pump(WidgetTester tester, Widget home) async {
  await tester.pumpWidget(MaterialApp(home: home));
  await tester.pumpAndSettle();
}

void main() {
  testWidgets('drone list shows serials and maintenance badges', (
    tester,
  ) async {
    final repository = _repository(
      (_) async => _json([
        _droneJson('AG10-002', 'Overdue'),
        _droneJson('AG16-003', 'DueSoon'),
      ]),
    );

    await _pump(tester, DroneListScreen(repository: repository));

    expect(find.text('AG10-002'), findsOneWidget);
    expect(find.text('Gecikmiş'), findsOneWidget);
    expect(find.text('Yaklaşıyor'), findsOneWidget);
  });

  testWidgets('drone list offers a retry when the request fails', (
    tester,
  ) async {
    var calls = 0;
    final repository = _repository((_) async {
      calls++;
      return calls == 1
          ? _json({'title': 'Sunucu hatası'}, 500)
          : _json([_droneJson('AG10-002', 'Ok')]);
    });

    await _pump(tester, DroneListScreen(repository: repository));
    expect(find.text('Sunucu hatası'), findsOneWidget);

    await tester.tap(find.text('Tekrar Dene'));
    await tester.pumpAndSettle();

    expect(find.text('AG10-002'), findsOneWidget);
  });

  testWidgets('ticket form shows server validation errors on their fields', (
    tester,
  ) async {
    final repository = _repository((request) async {
      if (request.method == 'POST') {
        return _json({
          'title': 'Gönderilen veriler geçersiz.',
          'errors': {
            'Title': ['Başlık zorunludur.'],
            'Description': ['Açıklama zorunludur.'],
          },
        }, 400);
      }
      return _json([_droneJson('AG10-002', 'Ok')]);
    });

    await _pump(
      tester,
      CreateTicketScreen(repository: repository, initialDroneId: _droneId),
    );
    await tester.tap(find.text('Talep Oluştur'));
    await tester.pumpAndSettle();

    expect(find.text('Başlık zorunludur.'), findsOneWidget);
    expect(find.text('Açıklama zorunludur.'), findsOneWidget);
  });

  testWidgets('ticket form requires a drone before sending', (tester) async {
    var posts = 0;
    final repository = _repository((request) async {
      if (request.method == 'POST') posts++;
      return _json([_droneJson('AG10-002', 'Ok')]);
    });

    await _pump(tester, CreateTicketScreen(repository: repository));
    await tester.tap(find.text('Talep Oluştur'));
    await tester.pumpAndSettle();

    expect(find.text('İHA seçilmelidir.'), findsOneWidget);
    expect(posts, 0);
  });
}
