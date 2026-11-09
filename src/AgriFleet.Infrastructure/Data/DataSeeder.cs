using System.Text.Json;
using AgriFleet.Core.Entities;
using AgriFleet.Core.Enums;
using AgriFleet.Core.Simulation;
using Microsoft.EntityFrameworkCore;

namespace AgriFleet.Infrastructure.Data;

public static class DataSeeder
{
    private const int FlightCount = 60;
    private const int FlightsWithTelemetry = 5;
    private const int TelemetryPointsPerFlight = 120;
    private const int SprayRows = 8;
    private const double LitersPerHectare = FlightPhysics.LitersPerHectare;

    public static async Task SeedAsync(AgriFleetDbContext db, DateTime now, CancellationToken ct = default)
    {
        if (await db.Drones.AnyAsync(ct))
            return;

        var random = new Random(42);
        var drones = CreateDrones(now);
        var fields = CreateFields();
        var flights = CreateFlights(drones, fields, now, random);
        var telemetry = flights
            .TakeLast(FlightsWithTelemetry)
            .SelectMany(f => CreateTelemetry(f, drones.First(d => d.Id == f.DroneId), fields.First(x => x.Id == f.FieldId)));

        db.Drones.AddRange(drones);
        db.Fields.AddRange(fields);
        db.Flights.AddRange(flights);
        db.TelemetryPoints.AddRange(telemetry);
        db.MaintenanceRecords.AddRange(CreateMaintenanceRecords(drones));
        db.ServiceTickets.AddRange(CreateTickets(drones, now));

        await db.SaveChangesAsync(ct);
    }

    private static List<Drone> CreateDrones(DateTime now)
    {
        Drone Create(string serial, string model, DroneStatus status, double tank, double total, double sinceService, int cycles, int serviceDaysAgo) => new()
        {
            Id = Guid.NewGuid(),
            SerialNumber = serial,
            Model = model,
            Status = status,
            TankCapacityLiters = tank,
            TotalFlightMinutes = total,
            FlightMinutesSinceMaintenance = sinceService,
            BatteryCycles = cycles,
            LastMaintenanceAt = now.AddDays(-serviceDaysAgo),
            CreatedAt = now.AddDays(-400),
        };

        return
        [
            Create("AG16-001", "AG-16", DroneStatus.Idle, 16, 4210, 640, 412, 25),
            Create("AG10-002", "AG-10", DroneStatus.Idle, 10, 6480, 3120, 655, 70),
            Create("AG16-003", "AG-16", DroneStatus.Idle, 16, 5330, 2780, 531, 40),
            Create("AG10-004", "AG-10", DroneStatus.Maintenance, 10, 3905, 120, 389, 5),
            Create("AG16-005", "AG-16", DroneStatus.Idle, 16, 2875, 900, 287, 30),
        ];
    }

    private static List<Field> CreateFields()
    {
        Field Create(string name, string crop, double hectares, double[][] boundary) => new()
        {
            Id = Guid.NewGuid(),
            Name = name,
            CropType = crop,
            AreaHectares = hectares,
            BoundaryJson = JsonSerializer.Serialize(boundary),
        };

        return
        [
            Create("Berdan Pamuk Parseli", "Pamuk", 46,
                [[36.925, 34.905], [36.931, 34.906], [36.930, 34.914], [36.924, 34.913]]),
            Create("Karabucak Narenciye Bahçesi", "Narenciye", 24,
                [[36.958, 34.862], [36.962, 34.863], [36.9615, 34.869], [36.9575, 34.868]]),
            Create("Çukurova Buğday Parseli", "Buğday", 86,
                [[36.941, 34.982], [36.949, 34.984], [36.948, 34.995], [36.940, 34.993]]),
            Create("Yenice Mısır Parseli", "Mısır", 39,
                [[36.972, 35.041], [36.977, 35.042], [36.9765, 35.050], [36.9715, 35.049]]),
        ];
    }

    private static List<Flight> CreateFlights(List<Drone> drones, List<Field> fields, DateTime now, Random random)
    {
        var flights = new List<Flight>(FlightCount);

        for (var i = 0; i < FlightCount; i++)
        {
            var hoursAgo = 1 + (FlightCount - 1 - i) * 12 + random.NextDouble() * 3;
            var drone = drones[random.Next(drones.Count)];

            if (drone.Status == DroneStatus.Maintenance && hoursAgo < 24 * 6)
                drone = drones[0];

            var duration = 12 + random.NextDouble() * 16;
            var liters = drone.TankCapacityLiters * (0.7 + random.NextDouble() * 0.3);
            var startedAt = now.AddHours(-hoursAgo);

            flights.Add(new Flight
            {
                Id = Guid.NewGuid(),
                DroneId = drone.Id,
                FieldId = fields[random.Next(fields.Count)].Id,
                StartedAt = startedAt,
                EndedAt = startedAt.AddMinutes(duration),
                DurationMinutes = Math.Round(duration, 2),
                SprayedHectares = Math.Round(liters / LitersPerHectare, 2),
                LiquidUsedLiters = Math.Round(liters, 2),
                MaxAltitudeM = Math.Round(3 + random.NextDouble() * 3, 2),
                Status = FlightStatus.Completed,
            });
        }

        return flights;
    }

    private static IEnumerable<TelemetryPoint> CreateTelemetry(Flight flight, Drone drone, Field field)
    {
        var boundary = JsonSerializer.Deserialize<double[][]>(field.BoundaryJson)!;
        var (minLat, maxLat) = (boundary.Min(p => p[0]), boundary.Max(p => p[0]));
        var (minLng, maxLng) = (boundary.Min(p => p[1]), boundary.Max(p => p[1]));
        var tankUsedPct = flight.LiquidUsedLiters / drone.TankCapacityLiters * 100;

        for (var i = 0; i < TelemetryPointsPerFlight; i++)
        {
            var progress = i / (double)(TelemetryPointsPerFlight - 1);
            var rowPosition = Math.Min(progress * SprayRows, SprayRows - 1e-9);
            var rowProgress = rowPosition - Math.Floor(rowPosition);
            var acrossRow = (int)rowPosition % 2 == 0 ? rowProgress : 1 - rowProgress;
            var rampFactor = Math.Min(1, Math.Min(progress, 1 - progress) * 20);

            yield return new TelemetryPoint
            {
                DroneId = flight.DroneId,
                FlightId = flight.Id,
                Timestamp = flight.StartedAt.AddMinutes(flight.DurationMinutes * progress),
                Lat = minLat + (maxLat - minLat) * progress,
                Lng = minLng + (maxLng - minLng) * acrossRow,
                AltitudeM = Math.Round(flight.MaxAltitudeM * rampFactor, 2),
                SpeedMs = Math.Round(5 * rampFactor, 2),
                BatteryPct = Math.Round(100 - 68 * progress, 1),
                TankLevelPct = Math.Round(100 - tankUsedPct * progress, 1),
            };
        }
    }

    private static IEnumerable<MaintenanceRecord> CreateMaintenanceRecords(List<Drone> drones)
    {
        foreach (var drone in drones)
        {
            var minutesAtService = drone.TotalFlightMinutes - drone.FlightMinutesSinceMaintenance;

            yield return new MaintenanceRecord
            {
                Id = Guid.NewGuid(),
                DroneId = drone.Id,
                Type = MaintenanceType.Scheduled,
                Description = "Periyodik bakım: motor, nozul ve pompa kontrolü",
                PerformedBy = "Servis Ekibi 1",
                PerformedAt = drone.LastMaintenanceAt,
                FlightMinutesAtService = minutesAtService,
            };

            yield return new MaintenanceRecord
            {
                Id = Guid.NewGuid(),
                DroneId = drone.Id,
                Type = MaintenanceType.Inspection,
                Description = "Sezon öncesi genel muayene",
                PerformedBy = "Servis Ekibi 2",
                PerformedAt = drone.LastMaintenanceAt.AddDays(-95),
                FlightMinutesAtService = Math.Max(0, minutesAtService - 1450),
            };
        }
    }

    private static List<ServiceTicket> CreateTickets(List<Drone> drones, DateTime now)
    {
        ServiceTicket Create(int droneIndex, string title, string description, TicketPriority priority, TicketStatus status, int createdDaysAgo, int? resolvedDaysAgo = null) => new()
        {
            Id = Guid.NewGuid(),
            DroneId = drones[droneIndex].Id,
            Title = title,
            Description = description,
            Priority = priority,
            Status = status,
            CreatedAt = now.AddDays(-createdDaysAgo),
            ResolvedAt = resolvedDaysAgo is null ? null : now.AddDays(-resolvedDaysAgo.Value),
        };

        return
        [
            Create(1, "Bakım süresi aşıldı", "Uçuş süresi limiti geçildi, periyodik bakım planlanmalı.", TicketPriority.Critical, TicketStatus.Open, 2),
            Create(3, "Pompa basıncı düşük", "İlaçlama sırasında pompa basıncında dalgalanma görüldü.", TicketPriority.High, TicketStatus.InProgress, 6),
            Create(2, "Nozul tıkanıklığı", "Sağ arka nozulda debi düşüşü bildirildi.", TicketPriority.Medium, TicketStatus.Open, 3),
            Create(0, "GPS sinyal uyarısı", "Tarla kenarında kısa süreli sinyal zayıflaması yaşandı.", TicketPriority.Low, TicketStatus.Resolved, 18, 15),
            Create(4, "Batarya konnektörü değişimi", "Konnektörde aşınma tespit edildi ve parça değiştirildi.", TicketPriority.Medium, TicketStatus.Resolved, 27, 24),
        ];
    }
}
