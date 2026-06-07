using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TurnoSys.Application.Features.Pacientes.Commands.ActualizarPaciente;
using TurnoSys.Application.Features.Pacientes.Commands.CrearPaciente;
using TurnoSys.Application.Features.Pacientes.Commands.EliminarPaciente;
using TurnoSys.Application.Features.Pacientes.Queries.GetPacienteById;
using TurnoSys.Application.Features.Pacientes.Queries.GetPacientes;
using TurnoSys.Domain.Interfaces.Services;

namespace TurnoSys.API.Controllers.v1;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class PacientesController(ISender sender, ICurrentUserService currentUser) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? busqueda,
        [FromQuery] bool? soloActivos,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        if (!currentUser.EmpresaId.HasValue) return Forbid();

        var result = await sender.Send(
            new GetPacientesQuery(currentUser.EmpresaId.Value, busqueda, soloActivos ?? true, page, pageSize), ct);

        return Ok(new { success = true, data = result.Items, pagination = new {
            result.Page, result.PageSize, result.TotalItems, result.TotalPages
        }});
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        if (!currentUser.EmpresaId.HasValue) return Forbid();

        var result = await sender.Send(new GetPacienteByIdQuery(id, currentUser.EmpresaId.Value), ct);
        if (result is null) return NotFound(new { success = false, error = "Paciente no encontrado." });

        return Ok(new { success = true, data = result });
    }

    [HttpPost]
    public async Task<IActionResult> Crear([FromBody] PacienteRequest request, CancellationToken ct)
    {
        if (!currentUser.EmpresaId.HasValue) return Forbid();

        var result = await sender.Send(new CrearPacienteCommand(
            currentUser.EmpresaId.Value,
            request.Nombre, request.Apellido, request.DNI,
            request.FechaNacimiento, request.Telefono, request.Email,
            request.Direccion, request.ObraSocial, request.NumeroAfiliado,
            request.ContactoEmergenciaNombre, request.ContactoEmergenciaTelefono,
            request.Observaciones, request.Restricciones), ct);

        if (result.Failure) return Conflict(new { success = false, error = result.Error });

        return CreatedAtAction(nameof(GetById), new { id = result.Value },
            new { success = true, data = new { id = result.Value } });
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Actualizar(Guid id, [FromBody] PacienteRequest request, CancellationToken ct)
    {
        if (!currentUser.EmpresaId.HasValue) return Forbid();

        var result = await sender.Send(new ActualizarPacienteCommand(
            id, currentUser.EmpresaId.Value,
            request.Nombre, request.Apellido, request.DNI,
            request.FechaNacimiento, request.Telefono, request.Email,
            request.Direccion, request.ObraSocial, request.NumeroAfiliado,
            request.ContactoEmergenciaNombre, request.ContactoEmergenciaTelefono,
            request.Observaciones, request.Restricciones,
            request.IsActivo ?? true), ct);

        if (result.Failure) return Conflict(new { success = false, error = result.Error });
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Eliminar(Guid id, CancellationToken ct)
    {
        if (!currentUser.EmpresaId.HasValue) return Forbid();

        var result = await sender.Send(new EliminarPacienteCommand(id, currentUser.EmpresaId.Value), ct);
        if (result.Failure) return Conflict(new { success = false, error = result.Error });
        return NoContent();
    }
}

public record PacienteRequest(
    string Nombre,
    string Apellido,
    string? DNI,
    DateOnly? FechaNacimiento,
    string? Telefono,
    string? Email,
    string? Direccion,
    string? ObraSocial,
    string? NumeroAfiliado,
    string? ContactoEmergenciaNombre,
    string? ContactoEmergenciaTelefono,
    string? Observaciones,
    string? Restricciones,
    bool? IsActivo = true
);
