using System.Linq.Expressions;
using AgriFleet.Core.Dtos;
using AgriFleet.Core.Entities;
using AgriFleet.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AgriFleet.Api.Controllers;

[ApiController]
[Route("api/flights")]
public class FlightsController(AgriFleetDbContext db) : ControllerBase
{
    private const int MaxPageSize = 100;

    private static readonly Expression<Func<Flight, FlightDto>> ToDto = f => new FlightDto(
        f.Id,
        f.DroneId,
        f.Drone.SerialNumber,
        f.FieldId,
        f.Field.Name,
        f.StartedAt,
        f.EndedAt,
        f.DurationMinutes,
        f.SprayedHectares,
        f.LiquidUsedLiters,
        f.MaxAltitudeM,
        f.Status,
        f.AbortReason);

    [HttpGet]
    public async Task<ActionResult<PagedResult<FlightDto>>> GetPage(
        [FromQuery] Guid? droneId,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        CancellationToken ct,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, MaxPageSize);

        var query = db.Flights.AsNoTracking();

        if (droneId is not null)
            query = query.Where(f => f.DroneId == droneId);

        if (from is not null)
        {
            var fromUtc = AsUtc(from.Value);
            query = query.Where(f => f.StartedAt >= fromUtc);
        }

        if (to is not null)
        {
            var toUtc = AsUtc(to.Value);
            query = query.Where(f => f.StartedAt <= toUtc);
        }

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(f => f.StartedAt)
            .ThenBy(f => f.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(ToDto)
            .ToListAsync(ct);

        return new PagedResult<FlightDto>(items, page, pageSize, totalCount);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<FlightDto>> GetById(Guid id, CancellationToken ct)
    {
        var flight = await db.Flights.AsNoTracking().Where(f => f.Id == id).Select(ToDto).FirstOrDefaultAsync(ct);

        return flight is null ? FlightNotFound() : flight;
    }

    [HttpGet("{id:guid}/telemetry")]
    public async Task<ActionResult<List<TelemetryPointDto>>> GetTelemetry(Guid id, CancellationToken ct)
    {
        if (!await db.Flights.AnyAsync(f => f.Id == id, ct))
            return FlightNotFound();

        return await db.TelemetryPoints
            .AsNoTracking()
            .Where(t => t.FlightId == id)
            .OrderBy(t => t.Timestamp)
            .Select(t => new TelemetryPointDto(t.Timestamp, t.Lat, t.Lng, t.AltitudeM, t.SpeedMs, t.BatteryPct, t.TankLevelPct))
            .ToListAsync(ct);
    }

    private static DateTime AsUtc(DateTime value) => value.Kind == DateTimeKind.Unspecified
        ? DateTime.SpecifyKind(value, DateTimeKind.Utc)
        : value.ToUniversalTime();

    private ObjectResult FlightNotFound() =>
        Problem(statusCode: StatusCodes.Status404NotFound, title: "Uçuş bulunamadı.");
}
