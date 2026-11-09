using System.Linq.Expressions;
using AgriFleet.Core.Dtos;
using AgriFleet.Core.Entities;
using AgriFleet.Core.Enums;
using AgriFleet.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AgriFleet.Api.Controllers;

[ApiController]
[Route("api/service-tickets")]
public class ServiceTicketsController(AgriFleetDbContext db) : ControllerBase
{
    private static readonly Expression<Func<ServiceTicket, ServiceTicketDto>> ToDto = t => new ServiceTicketDto(
        t.Id,
        t.DroneId,
        t.Drone.SerialNumber,
        t.Title,
        t.Description,
        t.Priority,
        t.Status,
        t.CreatedAt,
        t.ResolvedAt);

    [HttpGet]
    public async Task<ActionResult<List<ServiceTicketDto>>> GetAll(
        [FromQuery] TicketStatus? status,
        [FromQuery] Guid? droneId,
        CancellationToken ct)
    {
        var query = db.ServiceTickets.AsNoTracking();

        if (status is not null)
            query = query.Where(t => t.Status == status);

        if (droneId is not null)
            query = query.Where(t => t.DroneId == droneId);

        return await query.OrderByDescending(t => t.CreatedAt).Select(ToDto).ToListAsync(ct);
    }

    [HttpPost]
    public async Task<ActionResult<ServiceTicketDto>> Create(CreateServiceTicketRequest request, CancellationToken ct)
    {
        if (!await db.Drones.AnyAsync(d => d.Id == request.DroneId, ct))
            return Problem(statusCode: StatusCodes.Status404NotFound, title: "İHA bulunamadı.");

        var ticket = new ServiceTicket
        {
            Id = Guid.NewGuid(),
            DroneId = request.DroneId,
            Title = request.Title,
            Description = request.Description,
            Priority = request.Priority,
            Status = TicketStatus.Open,
            CreatedAt = DateTime.UtcNow,
        };

        db.ServiceTickets.Add(ticket);
        await db.SaveChangesAsync(ct);

        return StatusCode(StatusCodes.Status201Created, await LoadDto(ticket.Id, ct));
    }

    [HttpPatch("{id:guid}/status")]
    public async Task<ActionResult<ServiceTicketDto>> UpdateStatus(
        Guid id,
        UpdateServiceTicketStatusRequest request,
        CancellationToken ct)
    {
        var ticket = await db.ServiceTickets.FirstOrDefaultAsync(t => t.Id == id, ct);

        if (ticket is null)
            return Problem(statusCode: StatusCodes.Status404NotFound, title: "Servis talebi bulunamadı.");

        ticket.Status = request.Status;
        ticket.ResolvedAt = request.Status == TicketStatus.Resolved ? DateTime.UtcNow : null;
        await db.SaveChangesAsync(ct);

        return await LoadDto(id, ct);
    }

    private Task<ServiceTicketDto> LoadDto(Guid id, CancellationToken ct) =>
        db.ServiceTickets.AsNoTracking().Where(t => t.Id == id).Select(ToDto).FirstAsync(ct);
}
