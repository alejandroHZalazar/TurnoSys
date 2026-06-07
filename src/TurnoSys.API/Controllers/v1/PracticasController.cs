using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TurnoSys.Application.Common.Interfaces;
using TurnoSys.Application.Features.Practicas.Commands.ActualizarPractica;
using TurnoSys.Application.Features.Practicas.Commands.CrearPractica;
using TurnoSys.Application.Features.Practicas.Commands.EliminarPractica;
using TurnoSys.Application.Features.Practicas.Queries.GetPracticaById;
using TurnoSys.Application.Features.Practicas.Queries.GetPracticas;
using TurnoSys.Domain.Interfaces.Services;

namespace TurnoSys.API.Controllers.v1;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class PracticasController(ISender sender, ICurrentUserService currentUser, IApplicationDbContext db) : ControllerBase
{
    [HttpGet("categorias")]
    public async Task<IActionResult> GetCategorias(CancellationToken ct)
    {
        if (!currentUser.EmpresaId.HasValue) return Forbid();
        var cats = await db.CategoriasPracticas
            .AsNoTracking()
            .Where(c => c.EmpresaId == currentUser.EmpresaId.Value)
            .OrderBy(c => c.Orden).ThenBy(c => c.Nombre)
            .Select(c => new { c.Id, c.Nombre, c.Color })
            .ToListAsync(ct);
        return Ok(new { success = true, data = cats });
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] bool? soloActivas, CancellationToken ct)
    {
        if (!currentUser.EmpresaId.HasValue) return Forbid();
        var result = await sender.Send(new GetPracticasQuery(currentUser.EmpresaId.Value, soloActivas ?? true), ct);
        return Ok(new { success = true, data = result });
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        if (!currentUser.EmpresaId.HasValue) return Forbid();
        var result = await sender.Send(new GetPracticaByIdQuery(id, currentUser.EmpresaId.Value), ct);
        if (result is null) return NotFound(new { success = false, error = "Práctica no encontrada." });
        return Ok(new { success = true, data = result });
    }

    [HttpPost]
    public async Task<IActionResult> Crear([FromBody] PracticaRequest request, CancellationToken ct)
    {
        if (!currentUser.EmpresaId.HasValue) return Forbid();
        var result = await sender.Send(new CrearPracticaCommand(
            currentUser.EmpresaId.Value,
            request.CategoriaId, request.Nombre, request.Descripcion,
            request.Precio, request.DuracionMinutos, request.Color,
            request.RequiereObservaciones ?? false,
            request.RecordatorioRecDias), ct);

        if (result.Failure) return Conflict(new { success = false, error = result.Error });
        return CreatedAtAction(nameof(GetById), new { id = result.Value },
            new { success = true, data = new { id = result.Value } });
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Actualizar(Guid id, [FromBody] PracticaRequest request, CancellationToken ct)
    {
        if (!currentUser.EmpresaId.HasValue) return Forbid();
        var result = await sender.Send(new ActualizarPracticaCommand(
            id, currentUser.EmpresaId.Value,
            request.CategoriaId, request.Nombre, request.Descripcion,
            request.Precio, request.DuracionMinutos, request.Color,
            request.RequiereObservaciones ?? false,
            request.RecordatorioRecDias,
            request.IsActivo ?? true), ct);

        if (result.Failure) return Conflict(new { success = false, error = result.Error });
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Eliminar(Guid id, CancellationToken ct)
    {
        if (!currentUser.EmpresaId.HasValue) return Forbid();
        var result = await sender.Send(new EliminarPracticaCommand(id, currentUser.EmpresaId.Value), ct);
        if (result.Failure) return Conflict(new { success = false, error = result.Error });
        return NoContent();
    }
}

public record PracticaRequest(
    string Nombre,
    string? Descripcion,
    decimal Precio,
    int DuracionMinutos,
    string? Color,
    bool? RequiereObservaciones,
    int? RecordatorioRecDias,
    Guid? CategoriaId,
    bool? IsActivo
);
