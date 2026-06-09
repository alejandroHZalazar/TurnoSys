using MediatR;
using Microsoft.EntityFrameworkCore;
using TurnoSys.Application.Common.Interfaces;

namespace TurnoSys.Application.Features.Roles.Queries.GetRoles;

public class GetRolesQueryHandler(IApplicationDbContext db)
    : IRequestHandler<GetRolesQuery, List<RolDto>>
{
    public async Task<List<RolDto>> Handle(GetRolesQuery request, CancellationToken ct) =>
        await db.Roles
            .AsNoTracking()
            .OrderBy(r => r.Id)
            .Select(r => new RolDto(r.Id, r.Nombre, r.Descripcion, r.Permisos))
            .ToListAsync(ct);
}
