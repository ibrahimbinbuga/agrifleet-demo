# AgriFleet Technician App

A small Flutter app for field technicians: browse the fleet, add maintenance records and open service tickets. It uses the same REST API as the web dashboard.

Start the backend from the repository root with `docker compose up --build`, then run the app.

iOS simulator:

```bash
flutter run
```

Android emulator, where `10.0.2.2` is the host machine:

```bash
flutter run --dart-define=API_BASE_URL=http://10.0.2.2:8080
```

Tests:

```bash
flutter test
```
