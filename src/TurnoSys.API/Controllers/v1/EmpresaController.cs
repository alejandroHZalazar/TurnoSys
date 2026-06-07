using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TurnoSys.Application.Features.Empresa.Commands.ActualizarEmpresa;
using TurnoSys.Application.Features.Empresa.Commands.SubirLogo;
using TurnoSys.Application.Features.Empresa.Queries.GetEmpresa;
using TurnoSys.Application.Features.Empresa.Queries.GetLogo;
using TurnoSys.Domain.Interfaces.Services;

namespace TurnoSys.API.Controllers.v1;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class EmpresaController(ISender sender, ICurrentUserService currentUser) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken ct)
    {
        if (!currentUser.EmpresaId.HasValue) return Forbid();
        var result = await sender.Send(new GetEmpresaQuery(currentUser.EmpresaId.Value), ct);
        if (result is null) return NotFound(new { success = false, error = "Empresa no encontrada." });
        return Ok(new { success = true, data = result });
    }

    [HttpPut]
    public async Task<IActionResult> Actualizar([FromBody] EmpresaRequest request, CancellationToken ct)
    {
        if (!currentUser.EmpresaId.HasValue) return Forbid();
        var result = await sender.Send(new ActualizarEmpresaCommand(
            currentUser.EmpresaId.Value,
            request.RazonSocial, request.NombreFantasia, request.CUIT,
            request.Direccion, request.Telefono, request.Email,
            request.LogotipoUrl, request.SitioWeb,
            request.Instagram, request.Facebook, request.WhatsApp,
            request.HorarioDesde, request.HorarioHasta,
            request.TimeZone, request.Observaciones), ct);

        if (result.Failure) return Conflict(new { success = false, error = result.Error });
        return NoContent();
    }

    [HttpPost("logo")]
    [RequestSizeLimit(3 * 1024 * 1024)]
    public async Task<IActionResult> SubirLogo(IFormFile archivo, CancellationToken ct)
    {
        if (!currentUser.EmpresaId.HasValue) return Forbid();
        if (archivo is null || archivo.Length == 0)
            return BadRequest(new { success = false, error = "No se recibió ningún archivo." });

        using var ms = new MemoryStream();
        await archivo.CopyToAsync(ms, ct);

        var result = await sender.Send(new SubirLogoCommand(
            currentUser.EmpresaId.Value, ms.ToArray(), archivo.ContentType), ct);

        if (result.Failure) return Conflict(new { success = false, error = result.Error });
        return Ok(new { success = true });
    }

    [HttpGet("logo")]
    [AllowAnonymous]
    public async Task<IActionResult> GetLogo([FromQuery] Guid empresaId, CancellationToken ct)
    {
        // Permite cargar el logo por <img src> sin header Authorization.
        // El logo no es información sensible; se sirve por Id de empresa.
        var id = currentUser.EmpresaId ?? empresaId;
        if (id == Guid.Empty) return NotFound();

        var logo = await sender.Send(new GetLogoQuery(id), ct);
        if (logo is null) return NotFound();

        return File(logo.Contenido, logo.ContentType);
    }
}

public record EmpresaRequest(
    string RazonSocial,
    string? NombreFantasia,
    string? CUIT,
    string? Direccion,
    string? Telefono,
    string? Email,
    string? LogotipoUrl,
    string? SitioWeb,
    string? Instagram,
    string? Facebook,
    string? WhatsApp,
    string? HorarioDesde,
    string? HorarioHasta,
    string? TimeZone,
    string? Observaciones
);
