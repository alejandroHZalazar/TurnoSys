using MediatR;
using Microsoft.EntityFrameworkCore;
using TurnoSys.Application.Common.Interfaces;
using TurnoSys.Application.Common.Models;

namespace TurnoSys.Application.Features.Usuarios.Queries.GetUsuarios;

public class GetUsuariosQueryHandler(IApplicationDbContext db)
    : IRequestHandler<GetUsuariosQuery, PagedResult<UsuarioListDto>>
{
    public async Task<PagedResult<UsuarioListDto>> Handle(GetUsuariosQuery request, CancellationToken ct)
    {
        var query = db.Usuarios
            .AsNoTracking()
            .Include(u => u.Rol)
            .Where(u => !u.IsDeleted);

        if (request.SoloActivos == true)
            query = query.Where(u => u.IsActivo);

        if (!string.IsNullOrWhiteSpace(request.Busqueda))
        {
            var b = request.Busqueda.Trim().ToLower();
            query = query.Where(u =>
                u.NombreCompleto.ToLower().Contains(b) ||
                u.Email.ToLower().Contains(b));
        }

        var total = await query.CountAsync(ct);

        var items = await query
            .OrderBy(u => u.NombreCompleto)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(u => new UsuarioListDto(
                u.Id,
                u.NombreCompleto,
                u.Email,
                u.RolId,
                u.Rol.Nombre,
                u.IsActivo,
                u.UltimoAcceso,
                u.EmpresaId,
                u.ProfesionalId
            ))
            .ToListAsync(ct);

        return new PagedResult<UsuarioListDto>
        {
            Items = items,
            Page = request.Page,
            PageSize = request.PageSize,
            TotalItems = total
        };
    }
}
