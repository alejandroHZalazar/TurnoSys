using MediatR;
using TurnoSys.Application.Common.Models;

namespace TurnoSys.Application.Features.Usuarios.Queries.GetUsuarios;

public record GetUsuariosQuery(
    int Page = 1,
    int PageSize = 20,
    string? Busqueda = null,
    bool? SoloActivos = null
) : IRequest<PagedResult<UsuarioListDto>>;

public record UsuarioListDto(
    Guid Id,
    string NombreCompleto,
    string Email,
    int RolId,
    string RolNombre,
    bool IsActivo,
    DateTime? UltimoAcceso,
    Guid? EmpresaId,
    Guid? ProfesionalId
);
