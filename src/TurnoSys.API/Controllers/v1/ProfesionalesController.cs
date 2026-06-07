using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TurnoSys.Application.Features.Profesionales.Commands.ActualizarProfesional;
using TurnoSys.Application.Features.Profesionales.Commands.CrearProfesional;
using TurnoSys.Application.Features.Profesionales.Commands.EliminarProfesional;
using TurnoSys.Application.Features.Profesionales.Queries.GetProfesionalById;
using TurnoSys.Application.Features.Profesionales.Queries.GetProfesionales;
using TurnoSys.Domain.Interfaces.Services;

namespace TurnoSys.API.Controllers.v1;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class ProfesionalesController(ISender sender, ICurrentUserService currentUser) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] bool? soloActivos, CancellationToken ct)
    {
        if (!currentUser.EmpresaId.HasValue) return Forbid();
        var result = await sender.Send(new GetProfesionalesQuery(currentUser.EmpresaId.Value, soloActivos ?? true), ct);
        return Ok(new { success = true, data = result });
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        if (!currentUser.EmpresaId.HasValue) return Forbid();
        var result = await sender.Send(new GetProfesionalByIdQuery(id, currentUser.EmpresaId.Value), ct);
        if (result is null) return NotFound(new { success = false, error = "Profesional no encontrado." });
        return Ok(new { success = true, data = result });
    }

    [HttpPost]
    public async Task<IActionResult> Crear([FromBody] ProfesionalRequest request, CancellationToken ct)
    {
        if (!currentUser.EmpresaId.HasValue) return Forbid();
        var result = await sender.Send(new CrearProfesionalCommand(
            currentUser.EmpresaId.Value,
            request.Nombre, request.Apellido, request.Email, request.Telefono,
            request.Especialidad, request.Matricula,
            request.ColorAgenda ?? "#4F46E5",
            request.Observaciones,
            request.Horarios?.Select(h => new HorarioDto(h.DiaSemana, TimeOnly.Parse(h.HoraInicio), TimeOnly.Parse(h.HoraFin))).ToList() ?? []),
            ct);

        if (result.Failure) return Conflict(new { success = false, error = result.Error });
        return CreatedAtAction(nameof(GetById), new { id = result.Value },
            new { success = true, data = new { id = result.Value } });
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Actualizar(Guid id, [FromBody] ProfesionalRequest request, CancellationToken ct)
    {
        if (!currentUser.EmpresaId.HasValue) return Forbid();
        var result = await sender.Send(new ActualizarProfesionalCommand(
            id, currentUser.EmpresaId.Value,
            request.Nombre, request.Apellido, request.Email, request.Telefono,
            request.Especialidad, request.Matricula,
            request.ColorAgenda ?? "#4F46E5",
            request.Observaciones,
            request.IsActivo ?? true,
            request.Horarios?.Select(h => new HorarioDto(h.DiaSemana, TimeOnly.Parse(h.HoraInicio), TimeOnly.Parse(h.HoraFin))).ToList() ?? []),
            ct);

        if (result.Failure) return Conflict(new { success = false, error = result.Error });
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Eliminar(Guid id, CancellationToken ct)
    {
        if (!currentUser.EmpresaId.HasValue) return Forbid();
        var result = await sender.Send(new EliminarProfesionalCommand(id, currentUser.EmpresaId.Value), ct);
        if (result.Failure) return Conflict(new { success = false, error = result.Error });
        return NoContent();
    }
}

public record ProfesionalRequest(
    string Nombre,
    string Apellido,
    string? Email,
    string? Telefono,
    string? Especialidad,
    string? Matricula,
    string? ColorAgenda,
    string? Observaciones,
    bool? IsActivo = true,
    List<HorarioRequest>? Horarios = null
);

public record HorarioRequest(int DiaSemana, string HoraInicio, string HoraFin);
