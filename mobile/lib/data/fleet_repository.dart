import 'dart:async';
import 'dart:convert';

import 'package:http/http.dart' as http;

import 'models.dart';

class FleetRepository {
  FleetRepository({http.Client? client, String? baseUrl})
    : _client = client ?? http.Client(),
      _baseUrl = baseUrl ?? _defaultBaseUrl;

  static const _defaultBaseUrl = String.fromEnvironment(
    'API_BASE_URL',
    defaultValue: 'http://localhost:8080',
  );
  static const _timeout = Duration(seconds: 10);
  static const _jsonHeaders = {'Content-Type': 'application/json'};

  final http.Client _client;
  final String _baseUrl;

  Future<List<Drone>> getDrones() async {
    final body = await _send(() => _client.get(_uri('/api/drones')));
    return (body as List)
        .map((item) => Drone.fromJson(item as Map<String, dynamic>))
        .toList();
  }

  Future<Drone> getDrone(String id) async {
    final body = await _send(() => _client.get(_uri('/api/drones/$id')));
    return Drone.fromJson(body as Map<String, dynamic>);
  }

  Future<List<MaintenanceRecord>> getMaintenanceHistory(String droneId) async {
    final body = await _send(
      () => _client.get(_uri('/api/drones/$droneId/maintenance')),
    );
    return (body as List)
        .map((item) => MaintenanceRecord.fromJson(item as Map<String, dynamic>))
        .toList();
  }

  Future<void> createMaintenance({
    required String droneId,
    required String type,
    required String description,
    required String performedBy,
  }) async {
    await _send(
      () => _client.post(
        _uri('/api/drones/$droneId/maintenance'),
        headers: _jsonHeaders,
        body: jsonEncode({
          'type': type,
          'description': description,
          'performedBy': performedBy,
        }),
      ),
    );
  }

  Future<void> createTicket({
    required String droneId,
    required String title,
    required String description,
    required String priority,
  }) async {
    await _send(
      () => _client.post(
        _uri('/api/service-tickets'),
        headers: _jsonHeaders,
        body: jsonEncode({
          'droneId': droneId,
          'title': title,
          'description': description,
          'priority': priority,
        }),
      ),
    );
  }

  Uri _uri(String path) => Uri.parse('$_baseUrl$path');

  Future<Object?> _send(Future<http.Response> Function() request) async {
    final http.Response response;

    try {
      response = await request().timeout(_timeout);
    } on http.ClientException {
      throw const ApiException('Sunucuya bağlanılamadı.');
    } on TimeoutException {
      throw const ApiException('Sunucu zamanında yanıt vermedi.');
    }

    final text = utf8.decode(response.bodyBytes);

    if (response.statusCode >= 200 && response.statusCode < 300) {
      return text.isEmpty ? null : jsonDecode(text);
    }

    throw _toApiException(text);
  }

  ApiException _toApiException(String text) {
    const fallback = 'Beklenmeyen bir hata oluştu.';

    try {
      final problem = jsonDecode(text) as Map<String, dynamic>;
      final errors = (problem['errors'] as Map<String, dynamic>?) ?? const {};

      return ApiException(
        problem['title'] as String? ?? fallback,
        fieldErrors: errors.map(
          (field, messages) =>
              MapEntry(field, (messages as List).cast<String>()),
        ),
      );
    } on FormatException {
      return const ApiException(fallback);
    }
  }
}
