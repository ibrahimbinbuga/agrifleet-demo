using AgriFleet.Core.Dtos;
using AgriFleet.Core.Enums;
using AgriFleet.Core.Services;
using AgriFleet.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AgriFleet.Api.Controllers;

[ApiController]
[Route("api/dashboard")]
public class DashboardController(AgriFleetDbContext db) : ControllerBase
{
    [HttpGet("summary")]
    public async Task<ActionResult<DashboardSummaryDto>> GetSummary(CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        var today = DateTime.SpecifyKind(now.Date, DateTimeKind.Utc);
        var flightsToday = db.Flights.Where(f => f.StartedAt >= today);
        var drones = await db.Drones.AsNoTracking().ToListAsync(ct);

        return new DashboardSummaryDto(
            ActiveDrones: drones.Count(d => d.Status == DroneStatus.InFlight),
            FlightsToday: await flightsToday.CountAsync(ct),
            HectaresToday: Math.Round(await flightsToday.SumAsync(f => f.SprayedHectares, ct), 2),
            LiquidTodayLiters: Math.Round(await flightsToday.SumAsync(f => f.LiquidUsedLiters, ct), 2),
            MaintenanceDueCount: drones.Count(d => MaintenancePolicy.Evaluate(d, now).Status != MaintenanceStatus.Ok),
            OpenTickets: await db.ServiceTickets.CountAsync(t => t.Status != TicketStatus.Resolved, ct));
    }
}
