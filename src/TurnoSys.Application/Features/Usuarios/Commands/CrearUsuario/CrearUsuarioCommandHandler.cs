using MediatR;
using Microsoft.EntityFrameworkCore;
using TurnoSys.Application.Common.Interfaces;
using TurnoSys.Application.Common.Models;
using TurnoSys.Domain.Entities;
using TurnoSys.Domain.Interfaces.Services;

namespace TurnoSys.Application.Features.Usuarios.Commands.CrearUsuario;

public class CrearUsuarioCommandHandler(IApplicationDbContext db, IAuthService authService)
    : IRequestHandler<CrearUsuarioCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CrearUsuarioCommand request, CancellationToken ct)
    {
        var emailLower = request.Email.Trim().ToLower();

        var existe = await db.Usuarios.AnyAsync(u => u.Email == emailLower && !u.IsDeleted, ct);
        if (existe)
            return Result.Fail<Guid>("Ya existe un usuario con ese email.");

        var rolExiste = await db.Roles.AnyAsync(r => r.Id == request.RolId, ct);
        if (!rolExiste)
            return Result.Fail<Guid>("El rol especificado no existe.");

        var usuario = new Usuario
        {
            NombreCompleto = request.NombreCompleto.Trim(),
            Email          = emailLower,
            PasswordHash   = authService.HashPassword(request.Password),
            RolId          = request.RolId,
            EmpresaId      = request.EmpresaId,
            ProfesionalId  = request.ProfesionalId,
            IsActivo       = true
        };

        await db.Usuarios.AddAsync(usuario, ct);
        await db.SaveChangesAsync(ct);

        return Result.Ok(usuario.Id);
    }
}
