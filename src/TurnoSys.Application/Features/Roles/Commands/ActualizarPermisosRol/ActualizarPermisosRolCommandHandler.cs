using MediatR;
using Microsoft.EntityFrameworkCore;
using TurnoSys.Application.Common.Interfaces;
using TurnoSys.Application.Common.Models;

namespace TurnoSys.Application.Features.Roles.Commands.ActualizarPermisosRol;

public class ActualizarPermisosRolCommandHandler(IApplicationDbContext db)
    : IRequestHandler<ActualizarPermisosRolCommand, Result>
{
    public async Task<Result> Handle(ActualizarPermisosRolCommand request, CancellationToken ct)
    {
        var rol = await db.Roles.FirstOrDefaultAsync(r => r.Id == request.RolId, ct);
        if (rol is null)
            return Result.Fail("Rol no encontrado.");

        if (!string.IsNullOrWhiteSpace(request.Descripcion))
            rol.Descripcion = request.Descripcion.Trim();

        rol.Permisos = request.Permisos;

        await db.SaveChangesAsync(ct);
        return Result.Ok();
    }
}
