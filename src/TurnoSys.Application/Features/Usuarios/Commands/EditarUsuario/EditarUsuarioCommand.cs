using MediatR;
using TurnoSys.Application.Common.Models;

namespace TurnoSys.Application.Features.Usuarios.Commands.EditarUsuario;

public record EditarUsuarioCommand(
    Guid Id,
    string NombreCompleto,
    string Email,
    int RolId,
    Guid? EmpresaId,
    Guid? ProfesionalId,
    bool IsActivo
) : IRequest<Result>;
