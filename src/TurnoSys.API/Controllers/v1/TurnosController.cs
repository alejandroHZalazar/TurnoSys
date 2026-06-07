using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TurnoSys.Application.Features.Turnos.Commands.AtenderTurno;
using TurnoSys.Application.Features.Turnos.Commands.CancelarTurno;
using TurnoSys.Application.Features.Turnos.Commands.CrearTurno;
using TurnoSys.Application.Features.Turnos.Commands.ReagendarTurno;
using TurnoSys.Application.Features.Turnos.Queries.GetAgenda;
using TurnoSys.Domain.Interfaces.Services;

namespace TurnoSys.API.Controllers.v1;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class TurnosController(ISender sender, ICurrentUserService currentUser) : ControllerBase
{
    [HttpGet("agenda")]
    public async Task<IActionResult> GetAgenda(
        [FromQuery] DateTime desde,
        [FromQuery] DateTime hasta,
        [FromQuery] Guid? profesionalId,
        CancellationToken ct)
    {
        if (!currentUser.EmpresaId.HasValue)
            return Forbid();

        var result = await sender.Send(
            new GetAgendaQuery(currentUser.EmpresaId.Value, desde, hasta, profesionalId), ct);

        return Ok(new { success = true, data = result });
    }

    [HttpPost]
    public async Task<IActionResult> Crear([FromBody] CrearTurnoRequest request, CancellationToken ct)
    {
        if (!currentUser.EmpresaId.HasValue)
            return Forbid();

        var result = await sender.Send(new CrearTurnoCommand(
            currentUser.EmpresaId.Value,
            request.ProfesionalId,
            request.PacienteId,
            request.PracticaId,
            request.FechaHoraInicio,
            request.Observaciones,
            request.ProximoControlFecha), ct);

        if (result.Failure) return Conflict(new { success = false, error = result.Error });
        return CreatedAtAction(nameof(Crear), new { id = result.Value }, new { success = true, data = new { id = result.Value } });
    }

    [HttpPut("{id:guid}/cancelar")]
    public async Task<IActionResult> Cancelar(Guid id, [FromBody] CancelarRequest request, CancellationToken ct)
    {
        if (!currentUser.EmpresaId.HasValue) return Forbid();
        var result = await sender.Send(new CancelarTurnoCommand(id, currentUser.EmpresaId.Value, request.Motivo), ct);
        return result.Success ? NoContent() : Conflict(new { success = false, error = result.Error });
    }

    [HttpPut("{id:guid}/reagendar")]
    public async Task<IActionResult> Reagendar(Guid id, [FromBody] ReagendarRequest request, CancellationToken ct)
    {
        if (!currentUser.EmpresaId.HasValue) return Forbid();
        var result = await sender.Send(new ReagendarTurnoCommand(id, currentUser.EmpresaId.Value, request.NuevaFechaHoraInicio), ct);
        return result.Success ? NoContent() : Conflict(new { success = false, error = result.Error });
    }

    [HttpPut("{id:guid}/atender")]
    public async Task<IActionResult> Atender(Guid id, CancellationToken ct)
    {
        if (!currentUser.EmpresaId.HasValue) return Forbid();
        var result = await sender.Send(new AtenderTurnoCommand(id, currentUser.EmpresaId.Value), ct);
        return result.Success ? NoContent() : Conflict(new { success = false, error = result.Error });
    }
}

public record CrearTurnoRequest(
    Guid ProfesionalId,
    Guid PacienteId,
    Guid PracticaId,
    DateTime FechaHoraInicio,
    string? Observaciones,
    DateOnly? ProximoControlFecha
);

public record CancelarRequest(string? Motivo);
public record ReagendarRequest(DateTime NuevaFechaHoraInicio);
