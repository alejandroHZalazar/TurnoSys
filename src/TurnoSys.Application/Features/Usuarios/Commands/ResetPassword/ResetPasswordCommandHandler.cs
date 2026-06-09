using MediatR;
using Microsoft.EntityFrameworkCore;
using TurnoSys.Application.Common.Interfaces;
using TurnoSys.Application.Common.Models;
using TurnoSys.Domain.Interfaces.Services;

namespace TurnoSys.Application.Features.Usuarios.Commands.ResetPassword;

public class ResetPasswordCommandHandler(IApplicationDbContext db, IAuthService authService)
    : IRequestHandler<ResetPasswordCommand, Result>
{
    public async Task<Result> Handle(ResetPasswordCommand request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.NuevaPassword) || request.NuevaPassword.Length < 8)
            return Result.Fail("La contraseña debe tener al menos 8 caracteres.");

        var usuario = await db.Usuarios
            .FirstOrDefaultAsync(u => u.Id == request.UsuarioId && !u.IsDeleted, ct);

        if (usuario is null)
            return Result.Fail("Usuario no encontrado.");

        usuario.PasswordHash     = authService.HashPassword(request.NuevaPassword);
        usuario.IntentosFallidos = 0;
        usuario.BloqueadoHasta   = null;

        // Revocar todos los refresh tokens activos
        var tokens = await db.RefreshTokens
            .Where(t => t.UsuarioId == request.UsuarioId && !t.IsRevocado)
            .ToListAsync(ct);
        foreach (var t in tokens) { t.IsRevocado = true; t.FechaRevocacion = DateTime.UtcNow; }

        await db.SaveChangesAsync(ct);
        return Result.Ok();
    }
}
