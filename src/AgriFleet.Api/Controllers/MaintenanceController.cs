using AgriFleet.Core.Dtos;
using AgriFleet.Core.Entities;
using AgriFleet.Core.Enums;
using AgriFleet.Core.Services;
using AgriFleet.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AgriFleet.Api.Controllers;

[ApiController]
public class MaintenanceController(AgriFleetDbContext db) : ControllerBase
{
    [HttpGet("api/maintenance/due")]
    public async Task<ActionResult<List<DroneDto>>> GetDue(CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        var drones = await db.Drones.AsNoTracking().ToListAsync(ct);

        return drones
            .Where(d => MaintenancePolicy.Evaluate(d, now).Status != MaintenanceStatus.Ok)
            .OrderByDescending(d => MaintenancePolicy.Urgency(d, now))
            .Select(d => DroneDto.From(d, now))
            .ToList();
    }

    [HttpGet("api/drones/{droneId:guid}/maintenance")]
    public async Task<ActionResult<List<MaintenanceRecordDto>>> GetHistory(Guid droneId, CancellationToken ct)
    {
        if (!await db.Drones.AnyAsync(d => d.Id == droneId, ct))
            return DroneNotFound();

        return await db.MaintenanceRecords
            .AsNoTracking()
            .Where(m => m.DroneId == droneId)
            .OrderByDescending(m => m.PerformedAt)
            .Select(m => new MaintenanceRecordDto(m.Id, m.DroneId, m.Type, m.Description, m.PerformedBy, m.PerformedAt, m.FlightMinutesAtService))
            .ToListAsync(ct);
    }

    [HttpPost("api/drones/{droneId:guid}/maintenance")]
    public async Task<ActionResult<MaintenanceRecordDto>> Create(
        Guid droneId,
        CreateMaintenanceRecordRequest request,
        CancellationToken ct)
    {
        var drone = await db.Drones.FirstOrDefaultAsync(d => d.Id == droneId, ct);

        if (drone is null)
            return DroneNotFound();

        if (drone.Status == DroneStatus.InFlight)
            return Problem(statusCode: StatusCodes.Status409Conflict, title: "Uçuştaki İHA için bakım kaydı eklenemez.");

        var now = DateTime.UtcNow;
        var record = new MaintenanceRecord
        {
            Id = Guid.NewGuid(),
            DroneId = drone.Id,
            Type = request.Type,
            Description = request.Description,
            PerformedBy = request.PerformedBy,
            PerformedAt = now,
            FlightMinutesAtService = drone.TotalFlightMinutes,
        };

        drone.FlightMinutesSinceMaintenance = 0;
        drone.LastMaintenanceAt = now;
        drone.Status = DroneStatus.Idle;

        db.MaintenanceRecords.Add(record);
        await db.SaveChangesAsync(ct);

        var dto = new MaintenanceRecordDto(record.Id, record.DroneId, record.Type, record.Description, record.PerformedBy, record.PerformedAt, record.FlightMinutesAtService);

        return StatusCode(StatusCodes.Status201Created, dto);
    }

    private ObjectResult DroneNotFound() =>
        Problem(statusCode: StatusCodes.Status404NotFound, title: "İHA bulunamadı.");
}
