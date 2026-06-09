using MediatR;
using Microsoft.EntityFrameworkCore;
using TurnoSys.Application.Common.Interfaces;
using TurnoSys.Application.Common.Models;

namespace TurnoSys.Application.Features.Usuarios.Commands.DesactivarUsuario;

public class DesactivarUsuarioCommandHandler(IApplicationDbContext db)
    : IRequestHandler<DesactivarUsuarioCommand, Result>
{
    public async Task<Result> Handle(DesactivarUsuarioCommand request, CancellationToken ct)
    {
        var usuario = await db.Usuarios
            .FirstOrDefaultAsync(u => u.Id == request.Id && !u.IsDeleted, ct);

        if (usuario is null)
            return Result.Fail("Usuario no encontrado.");

        // Proteger al SuperAdmin único
        if (usuario.RolId == 1)
        {
            var otrosSuperAdmin = await db.Usuarios.CountAsync(u =>
                u.RolId == 1 && u.IsActivo && u.Id != request.Id && !u.IsDeleted, ct);
            if (otrosSuperAdmin == 0)
                return Result.Fail("No se puede desactivar el único SuperAdmin del sistema.");
        }

        usuario.IsActivo = false;
        await db.SaveChangesAsync(ct);
        return Result.Ok();
    }
}
