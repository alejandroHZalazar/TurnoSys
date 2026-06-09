using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TurnoSys.Application.Features.Roles.Commands.ActualizarPermisosRol;
using TurnoSys.Application.Features.Roles.Queries.GetRoles;

namespace TurnoSys.API.Controllers.v1;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class RolesController(ISender sender) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var result = await sender.Send(new GetRolesQuery(), ct);
        return Ok(new { success = true, data = result });
    }

    [HttpPut("{id:int}/permisos")]
    public async Task<IActionResult> ActualizarPermisos(int id, [FromBody] ActualizarPermisosRequest request, CancellationToken ct)
    {
        var result = await sender.Send(new ActualizarPermisosRolCommand(id, request.Descripcion, request.Permisos), ct);
        if (result.Failure)
            return BadRequest(new { success = false, error = result.Error });
        return Ok(new { success = true });
    }
}

public record ActualizarPermisosRequest(string? Descripcion, string? Permisos);
