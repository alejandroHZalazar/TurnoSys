using MediatR;
using TurnoSys.Application.Common.Models;

namespace TurnoSys.Application.Features.Usuarios.Commands.CrearUsuario;

public record CrearUsuarioCommand(
    string NombreCompleto,
    string Email,
    string Password,
    int RolId,
    Guid? EmpresaId,
    Guid? ProfesionalId
) : IRequest<Result<Guid>>;
