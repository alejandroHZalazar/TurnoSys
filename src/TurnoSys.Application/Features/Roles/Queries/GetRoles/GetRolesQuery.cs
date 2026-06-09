using MediatR;

namespace TurnoSys.Application.Features.Roles.Queries.GetRoles;

public record GetRolesQuery : IRequest<List<RolDto>>;

public record RolDto(
    int Id,
    string Nombre,
    string? Descripcion,
    string? Permisos
);
