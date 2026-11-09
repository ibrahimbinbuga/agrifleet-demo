using System.Text.Json;
using AgriFleet.Api.Hubs;
using AgriFleet.Core.Dtos;
using AgriFleet.Core.Entities;
using AgriFleet.Core.Enums;
using AgriFleet.Core.Services;
using AgriFleet.Core.Simulation;
using AgriFleet.Infrastructure.Data;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace AgriFleet.Api.Simulator;

public class DroneSimulator(
    IServiceScopeFactory scopeFactory,
    IHubContext<TelemetryHub, ITelemetryClient> hub,
    IOptions<SimulatorOptions> options,
    ILogger<DroneSimulator> logger) : BackgroundService
{
    private const int TicksPerTelemetryWrite = 5;
    private const string LowBatteryAbortReason = "Düşük batarya";
    private const string RestartAbortReason = "Sunucu yeniden başlatıldı";

    private static readonly TimeSpan TickInterval = TimeSpan.FromSeconds(1);

    private readonly SimulatorOptions _options = options.Value;
    private readonly Dictionary<Guid, ActiveFlight> _active = [];
    private readonly List<TelemetryPoint> _pendingTelemetry = [];
    private readonly Random _random = new();
    private int _tick;

    private sealed record ActiveFlight(Guid DroneId, string SerialNumber, Guid FlightId, double TankCapacityLiters, FlightSimulation Simulation);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_options.Enabled)
            return;

        await RecoverInterruptedFlightsAsync(stoppingToken);

        using var timer = new PeriodicTimer(TickInterval);

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                await TickAsync(stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogError(ex, "Simulator tick failed");
            }
        }
    }

    private async Task RecoverInterruptedFlightsAsync(CancellationToken ct)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AgriFleetDbContext>();
        var now = DateTime.UtcNow;

        await db.Flights
            .Where(f => f.Status == FlightStatus.InProgress)
            .ExecuteUpdateAsync(s => s
                .SetProperty(f => f.Status, FlightStatus.Aborted)
                .SetProperty(f => f.EndedAt, now)
                .SetProperty(f => f.AbortReason, RestartAbortReason), ct);

        await db.Drones
            .Where(d => d.Status == DroneStatus.InFlight || d.Status == DroneStatus.Charging)
            .ExecuteUpdateAsync(s => s.SetProperty(d => d.Status, DroneStatus.Idle), ct);
    }

    private async Task TickAsync(CancellationToken ct)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AgriFleetDbContext>();
        var now = DateTime.UtcNow;
        var live = new List<LiveTelemetryDto>();

        foreach (var flight in _active.Values.ToList())
        {
            var events = flight.Simulation.Step(_options.TimeScale * TickInterval.TotalSeconds);

            if (flight.Simulation.IsAirborne)
                RecordTelemetry(flight, now, live);

            foreach (var flightEvent in events)
                await HandleEventAsync(db, flight, flightEvent, now, ct);
        }

        await hub.Clients.All.TelemetryBatch(live);

        if (++_tick % TicksPerTelemetryWrite == 0)
            await WritePendingTelemetryAsync(db, ct);

        await TryStartFlightAsync(db, now, ct);
    }

    private void RecordTelemetry(ActiveFlight flight, DateTime now, List<LiveTelemetryDto> live)
    {
        var simulation = flight.Simulation;

        live.Add(new LiveTelemetryDto(
            flight.DroneId,
            flight.SerialNumber,
            flight.FlightId,
            simulation.Phase,
            now,
            simulation.Position.Lat,
            simulation.Position.Lng,
            Math.Round(simulation.AltitudeM, 2),
            Math.Round(simulation.SpeedMs, 2),
            Math.Round(simulation.BatteryPct, 1),
            Math.Round(simulation.TankLevelPct, 1)));

        _pendingTelemetry.Add(new TelemetryPoint
        {
            DroneId = flight.DroneId,
            FlightId = flight.FlightId,
            Timestamp = now,
            Lat = simulation.Position.Lat,
            Lng = simulation.Position.Lng,
            AltitudeM = Math.Round(simulation.AltitudeM, 2),
            SpeedMs = Math.Round(simulation.SpeedMs, 2),
            BatteryPct = Math.Round(simulation.BatteryPct, 1),
            TankLevelPct = Math.Round(simulation.TankLevelPct, 1),
        });
    }

    private async Task WritePendingTelemetryAsync(AgriFleetDbContext db, CancellationToken ct)
    {
        if (_pendingTelemetry.Count == 0)
            return;

        db.TelemetryPoints.AddRange(_pendingTelemetry);
        await db.SaveChangesAsync(ct);
        _pendingTelemetry.Clear();
    }

    private async Task HandleEventAsync(AgriFleetDbContext db, ActiveFlight flight, FlightEvent flightEvent, DateTime now, CancellationToken ct)
    {
        switch (flightEvent)
        {
            case FlightEvent.LowBattery:
                await RaiseAlertAsync(flight, AlertSeverity.Warning, $"Batarya %{flight.Simulation.BatteryPct:F0} seviyesine düştü.", now);
                break;

            case FlightEvent.TankEmpty:
                await RaiseAlertAsync(flight, AlertSeverity.Warning, "İlaç tankı boşaldı, üsse dönüyor.", now);
                break;

            case FlightEvent.Aborted:
                await RaiseAlertAsync(flight, AlertSeverity.Critical, "Düşük batarya nedeniyle görev iptal edildi.", now);
                break;

            case FlightEvent.Landed:
                await FinalizeFlightAsync(db, flight, now, ct);
                break;

            case FlightEvent.Charged:
                await CompleteChargingAsync(db, flight, ct);
                break;
        }
    }

    private async Task FinalizeFlightAsync(AgriFleetDbContext db, ActiveFlight flight, DateTime now, CancellationToken ct)
    {
        var record = await db.Flights.FirstAsync(f => f.Id == flight.FlightId, ct);
        var drone = await db.Drones.FirstAsync(d => d.Id == flight.DroneId, ct);
        var simulation = flight.Simulation;
        var wasOverdue = MaintenancePolicy.Evaluate(drone, now).Status == MaintenanceStatus.Overdue;
        var minutes = simulation.FlightSeconds / 60;
        var liters = flight.TankCapacityLiters * (100 - simulation.TankLevelPct) / 100;

        record.EndedAt = now;
        record.DurationMinutes = Math.Round(minutes, 2);
        record.LiquidUsedLiters = Math.Round(liters, 2);
        record.SprayedHectares = Math.Round(liters / FlightPhysics.LitersPerHectare, 2);
        record.MaxAltitudeM = Math.Round(simulation.MaxAltitudeM, 2);
        record.Status = simulation.IsAborted ? FlightStatus.Aborted : FlightStatus.Completed;
        record.AbortReason = simulation.IsAborted ? LowBatteryAbortReason : null;

        drone.TotalFlightMinutes += minutes;
        drone.FlightMinutesSinceMaintenance += minutes;
        drone.BatteryCycles++;
        drone.Status = DroneStatus.Charging;

        await db.SaveChangesAsync(ct);
        await hub.Clients.All.DroneStatusChanged(new DroneStatusChangedDto(drone.Id, drone.Status));

        var evaluation = MaintenancePolicy.Evaluate(drone, now);

        if (!wasOverdue && evaluation.Status == MaintenanceStatus.Overdue)
            await RaiseAlertAsync(flight, AlertSeverity.Critical, $"Bakım gecikti: {evaluation.Reason}", now);
    }

    private async Task CompleteChargingAsync(AgriFleetDbContext db, ActiveFlight flight, CancellationToken ct)
    {
        _active.Remove(flight.DroneId);

        var drone = await db.Drones.FirstAsync(d => d.Id == flight.DroneId, ct);

        if (drone.Status != DroneStatus.Charging)
            return;

        drone.Status = DroneStatus.Idle;
        await db.SaveChangesAsync(ct);
        await hub.Clients.All.DroneStatusChanged(new DroneStatusChangedDto(drone.Id, drone.Status));
    }

    private async Task TryStartFlightAsync(AgriFleetDbContext db, DateTime now, CancellationToken ct)
    {
        if (_active.Values.Count(f => f.Simulation.IsAirborne) >= _options.MaxConcurrentFlights)
            return;

        if (_random.NextDouble() >= _options.StartChancePerTick)
            return;

        var busyDroneIds = _active.Keys.ToList();
        var candidates = await db.Drones
            .Where(d => d.Status == DroneStatus.Idle && !busyDroneIds.Contains(d.Id))
            .ToListAsync(ct);
        var fields = await db.Fields.AsNoTracking().ToListAsync(ct);

        if (candidates.Count == 0 || fields.Count == 0)
            return;

        var drone = candidates[_random.Next(candidates.Count)];
        var field = fields[_random.Next(fields.Count)];
        var boundary = JsonSerializer.Deserialize<double[][]>(field.BoundaryJson)!
            .Select(p => new GeoPoint(p[0], p[1]))
            .ToList();

        var flight = new Flight
        {
            Id = Guid.NewGuid(),
            DroneId = drone.Id,
            FieldId = field.Id,
            StartedAt = now,
            Status = FlightStatus.InProgress,
        };

        drone.Status = DroneStatus.InFlight;
        db.Flights.Add(flight);
        await db.SaveChangesAsync(ct);

        _active[drone.Id] = new ActiveFlight(
            drone.Id,
            drone.SerialNumber,
            flight.Id,
            drone.TankCapacityLiters,
            new FlightSimulation(FlightPlan.Create(boundary, _random)));

        await hub.Clients.All.DroneStatusChanged(new DroneStatusChangedDto(drone.Id, drone.Status));
    }

    private Task RaiseAlertAsync(ActiveFlight flight, AlertSeverity severity, string message, DateTime now) =>
        hub.Clients.All.AlertRaised(new DroneAlertDto(flight.DroneId, flight.SerialNumber, severity, message, now));
}
