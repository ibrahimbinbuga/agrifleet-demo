using System.Text.Json;
using AgriFleet.Core.Dtos;
using AgriFleet.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AgriFleet.Api.Controllers;

[ApiController]
[Route("api/fields")]
public class FieldsController(AgriFleetDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<FieldDto>>> GetAll(CancellationToken ct)
    {
        var fields = await db.Fields.AsNoTracking().OrderBy(f => f.Name).ToListAsync(ct);

        return fields
            .Select(f => new FieldDto(
                f.Id,
                f.Name,
                f.CropType,
                f.AreaHectares,
                JsonSerializer.Deserialize<double[][]>(f.BoundaryJson)!))
            .ToList();
    }
}
