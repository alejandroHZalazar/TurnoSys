using MediatR;
using TurnoSys.Application.Common.Models;

namespace TurnoSys.Application.Features.Roles.Commands.ActualizarPermisosRol;

public record ActualizarPermisosRolCommand(
    int RolId,
    string? Descripcion,
    string? Permisos   // JSON array serializado, o null para acceso total
) : IRequest<Result>;
