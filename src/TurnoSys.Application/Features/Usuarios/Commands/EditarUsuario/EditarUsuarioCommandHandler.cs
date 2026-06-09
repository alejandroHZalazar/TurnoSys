using MediatR;
using Microsoft.EntityFrameworkCore;
using TurnoSys.Application.Common.Interfaces;
using TurnoSys.Application.Common.Models;

namespace TurnoSys.Application.Features.Usuarios.Commands.EditarUsuario;

public class EditarUsuarioCommandHandler(IApplicationDbContext db)
    : IRequestHandler<EditarUsuarioCommand, Result>
{
    public async Task<Result> Handle(EditarUsuarioCommand request, CancellationToken ct)
    {
        var usuario = await db.Usuarios
            .FirstOrDefaultAsync(u => u.Id == request.Id && !u.IsDeleted, ct);

        if (usuario is null)
            return Result.Fail("Usuario no encontrado.");

        var emailLower = request.Email.Trim().ToLower();
        var emailEnUso = await db.Usuarios.AnyAsync(u =>
            u.Email == emailLower && u.Id != request.Id && !u.IsDeleted, ct);
        if (emailEnUso)
            return Result.Fail("Ya existe otro usuario con ese email.");

        var rolExiste = await db.Roles.AnyAsync(r => r.Id == request.RolId, ct);
        if (!rolExiste)
            return Result.Fail("El rol especificado no existe.");

        usuario.NombreCompleto = request.NombreCompleto.Trim();
        usuario.Email          = emailLower;
        usuario.RolId          = request.RolId;
        usuario.EmpresaId      = request.EmpresaId;
        usuario.ProfesionalId  = request.ProfesionalId;
        usuario.IsActivo       = request.IsActivo;

        await db.SaveChangesAsync(ct);
        return Result.Ok();
    }
}
