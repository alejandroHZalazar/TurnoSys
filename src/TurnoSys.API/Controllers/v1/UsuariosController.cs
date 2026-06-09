using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TurnoSys.Application.Features.Usuarios.Commands.CrearUsuario;
using TurnoSys.Application.Features.Usuarios.Commands.DesactivarUsuario;
using TurnoSys.Application.Features.Usuarios.Commands.EditarUsuario;
using TurnoSys.Application.Features.Usuarios.Commands.ResetPassword;
using TurnoSys.Application.Features.Usuarios.Queries.GetUsuarios;

namespace TurnoSys.API.Controllers.v1;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class UsuariosController(ISender sender) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? busqueda = null,
        [FromQuery] bool? soloActivos = null,
        CancellationToken ct = default)
    {
        var result = await sender.Send(new GetUsuariosQuery(page, pageSize, busqueda, soloActivos), ct);
        return Ok(new { success = true, data = result });
    }

    [HttpPost]
    public async Task<IActionResult> Crear([FromBody] CrearUsuarioCommand command, CancellationToken ct)
    {
        var result = await sender.Send(command, ct);
        if (result.Failure)
            return BadRequest(new { success = false, error = result.Error });
        return Ok(new { success = true, data = result.Value });
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Editar(Guid id, [FromBody] EditarUsuarioRequest request, CancellationToken ct)
    {
        var result = await sender.Send(new EditarUsuarioCommand(
            id,
            request.NombreCompleto,
            request.Email,
            request.RolId,
            request.EmpresaId,
            request.ProfesionalId,
            request.IsActivo
        ), ct);

        if (result.Failure)
            return BadRequest(new { success = false, error = result.Error });
        return Ok(new { success = true });
    }

    [HttpPatch("{id:guid}/desactivar")]
    public async Task<IActionResult> Desactivar(Guid id, CancellationToken ct)
    {
        var result = await sender.Send(new DesactivarUsuarioCommand(id), ct);
        if (result.Failure)
            return BadRequest(new { success = false, error = result.Error });
        return Ok(new { success = true });
    }

    [HttpPatch("{id:guid}/reset-password")]
    public async Task<IActionResult> ResetPassword(Guid id, [FromBody] ResetPasswordRequest request, CancellationToken ct)
    {
        var result = await sender.Send(new ResetPasswordCommand(id, request.NuevaPassword), ct);
        if (result.Failure)
            return BadRequest(new { success = false, error = result.Error });
        return Ok(new { success = true });
    }
}

public record EditarUsuarioRequest(
    string NombreCompleto,
    string Email,
    int RolId,
    Guid? EmpresaId,
    Guid? ProfesionalId,
    bool IsActivo
);

public record ResetPasswordRequest(string NuevaPassword);
