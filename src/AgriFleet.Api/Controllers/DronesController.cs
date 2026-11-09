using AgriFleet.Api.Hubs;
using AgriFleet.Core.Dtos;
using AgriFleet.Core.Entities;
using AgriFleet.Core.Enums;
using AgriFleet.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace AgriFleet.Api.Controllers;

[ApiController]
[Route("api/drones")]
public class DronesController(AgriFleetDbContext db, IHubContext<TelemetryHub, ITelemetryClient> hub) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<DroneDto>>> GetAll(CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        var drones = await db.Drones.AsNoTracking().OrderBy(d => d.SerialNumber).ToListAsync(ct);

        return drones.Select(d => DroneDto.From(d, now)).ToList();
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<DroneDto>> GetById(Guid id, CancellationToken ct)
    {
        var drone = await db.Drones.AsNoTracking().FirstOrDefaultAsync(d => d.Id == id, ct);

        return drone is null ? DroneNotFound() : DroneDto.From(drone, DateTime.UtcNow);
    }

    [HttpPost]
    public async Task<ActionResult<DroneDto>> Create(CreateDroneRequest request, CancellationToken ct)
    {
        if (await db.Drones.AnyAsync(d => d.SerialNumber == request.SerialNumber, ct))
            return Problem(statusCode: StatusCodes.Status409Conflict, title: "Bu seri numarası zaten kayıtlı.");

        var now = DateTime.UtcNow;
        var drone = new Drone
        {
            Id = Guid.NewGuid(),
            SerialNumber = request.SerialNumber,
            Model = request.Model,
            Status = DroneStatus.Idle,
            TankCapacityLiters = request.TankCapacityLiters,
            LastMaintenanceAt = now,
            CreatedAt = now,
        };

        db.Drones.Add(drone);
        await db.SaveChangesAsync(ct);

        return CreatedAtAction(nameof(GetById), new { id = drone.Id }, DroneDto.From(drone, now));
    }

    [HttpPatch("{id:guid}/status")]
    public async Task<ActionResult<DroneDto>> UpdateStatus(Guid id, UpdateDroneStatusRequest request, CancellationToken ct)
    {
        var drone = await db.Drones.FirstOrDefaultAsync(d => d.Id == id, ct);

        if (drone is null)
            return DroneNotFound();

        if (drone.Status == DroneStatus.InFlight || request.Status == DroneStatus.InFlight)
            return Problem(statusCode: StatusCodes.Status409Conflict, title: "Uçuş durumu elle değiştirilemez.");

        drone.Status = request.Status;
        await db.SaveChangesAsync(ct);
        await hub.Clients.All.DroneStatusChanged(new DroneStatusChangedDto(drone.Id, drone.Status));

        return DroneDto.From(drone, DateTime.UtcNow);
    }

    private ObjectResult DroneNotFound() =>
        Problem(statusCode: StatusCodes.Status404NotFound, title: "İHA bulunamadı.");
}
