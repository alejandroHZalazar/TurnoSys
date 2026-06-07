using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TurnoSys.Application.Features.Estadisticas.Queries.GetDashboardKPIs;
using TurnoSys.Application.Features.Estadisticas.Queries.GetEstadisticasPacientes;
using TurnoSys.Application.Features.Estadisticas.Queries.GetEstadisticasProfesionales;
using TurnoSys.Application.Features.Estadisticas.Queries.GetHeatmapSemanal;
using TurnoSys.Application.Features.Estadisticas.Queries.GetTurnosPorPeriodo;
using TurnoSys.Domain.Interfaces.Services;

namespace TurnoSys.API.Controllers.v1;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class EstadisticasController(ISender sender, ICurrentUserService currentUser) : ControllerBase
{
    [HttpGet("kpis")]
    public async Task<IActionResult> GetKPIs(
        [FromQuery] DateTime desde, [FromQuery] DateTime hasta, CancellationToken ct)
    {
        if (!currentUser.EmpresaId.HasValue) return Forbid();
        var result = await sender.Send(new GetDashboardKPIsQuery(currentUser.EmpresaId.Value, desde, hasta), ct);
        return Ok(new { success = true, data = result });
    }

    [HttpGet("turnos-por-periodo")]
    public async Task<IActionResult> GetTurnosPorPeriodo(
        [FromQuery] DateTime desde, [FromQuery] DateTime hasta,
        [FromQuery] Guid? profesionalId, CancellationToken ct)
    {
        if (!currentUser.EmpresaId.HasValue) return Forbid();
        var result = await sender.Send(
            new GetTurnosPorPeriodoQuery(currentUser.EmpresaId.Value, desde, hasta, profesionalId), ct);
        return Ok(new { success = true, data = result });
    }

    [HttpGet("heatmap")]
    public async Task<IActionResult> GetHeatmap(
        [FromQuery] int diasAtras = 90, [FromQuery] Guid? profesionalId = null, CancellationToken ct = default)
    {
        if (!currentUser.EmpresaId.HasValue) return Forbid();
        var result = await sender.Send(
            new GetHeatmapSemanalQuery(currentUser.EmpresaId.Value, diasAtras, profesionalId), ct);
        return Ok(new { success = true, data = result });
    }

    [HttpGet("profesionales")]
    public async Task<IActionResult> GetProfesionales(
        [FromQuery] DateTime desde, [FromQuery] DateTime hasta, CancellationToken ct)
    {
        if (!currentUser.EmpresaId.HasValue) return Forbid();
        var result = await sender.Send(
            new GetEstadisticasProfesionalesQuery(currentUser.EmpresaId.Value, desde, hasta), ct);
        return Ok(new { success = true, data = result });
    }

    [HttpGet("pacientes")]
    public async Task<IActionResult> GetPacientes(
        [FromQuery] DateTime desde, [FromQuery] DateTime hasta, CancellationToken ct)
    {
        if (!currentUser.EmpresaId.HasValue) return Forbid();
        var result = await sender.Send(
            new GetEstadisticasPacientesQuery(currentUser.EmpresaId.Value, desde, hasta), ct);
        return Ok(new { success = true, data = result });
    }
}
