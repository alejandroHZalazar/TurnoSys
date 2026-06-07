using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TurnoSys.Application.Features.Parametros.Commands.ActualizarConfiguracion;
using TurnoSys.Application.Features.Parametros.Commands.ProbarEmail;
using TurnoSys.Application.Features.Parametros.Queries.GetConfiguracion;
using TurnoSys.Domain.Interfaces.Services;

namespace TurnoSys.API.Controllers.v1;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class ConfiguracionController(ISender sender, ICurrentUserService currentUser) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken ct)
    {
        if (!currentUser.EmpresaId.HasValue) return Forbid();
        var result = await sender.Send(new GetConfiguracionQuery(currentUser.EmpresaId.Value), ct);
        return Ok(new { success = true, data = result });
    }

    [HttpPut]
    public async Task<IActionResult> Actualizar([FromBody] List<ParametroUpdate> parametros, CancellationToken ct)
    {
        if (!currentUser.EmpresaId.HasValue) return Forbid();
        var result = await sender.Send(
            new ActualizarConfiguracionCommand(currentUser.EmpresaId.Value, parametros ?? []), ct);
        if (result.Failure) return Conflict(new { success = false, error = result.Error });
        return NoContent();
    }

    [HttpPost("probar-email")]
    public async Task<IActionResult> ProbarEmail([FromBody] ProbarEmailRequest request, CancellationToken ct)
    {
        if (!currentUser.EmpresaId.HasValue) return Forbid();
        var destino = string.IsNullOrWhiteSpace(request.Email) ? currentUser.Email : request.Email;
        if (string.IsNullOrWhiteSpace(destino))
            return BadRequest(new { success = false, error = "No hay email de destino." });

        var result = await sender.Send(new ProbarEmailCommand(destino), ct);
        if (result.Failure) return Conflict(new { success = false, error = result.Error });
        return Ok(new { success = true });
    }
}

public record ProbarEmailRequest(string? Email);
