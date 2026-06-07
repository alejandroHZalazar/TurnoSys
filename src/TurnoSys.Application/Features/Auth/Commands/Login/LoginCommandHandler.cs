using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TurnoSys.Application.Common.Interfaces;
using TurnoSys.Application.Common.Models;
using TurnoSys.Domain.Interfaces.Services;

namespace TurnoSys.Application.Features.Auth.Commands.Login;

public class LoginCommandHandler(
    IApplicationDbContext db,
    IAuthService authService,
    ILogger<LoginCommandHandler> logger) : IRequestHandler<LoginCommand, Result<LoginResponse>>
{
    public async Task<Result<LoginResponse>> Handle(LoginCommand request, CancellationToken ct)
    {
        var usuario = await db.Usuarios
            .Include(u => u.Rol)
            .FirstOrDefaultAsync(u => u.Email == request.Email.ToLower() && !u.IsDeleted, ct);

        if (usuario is null)
            return Result.Fail<LoginResponse>("Credenciales inválidas.");

        if (!usuario.IsActivo)
            return Result.Fail<LoginResponse>("La cuenta está desactivada.");

        if (usuario.BloqueadoHasta.HasValue && usuario.BloqueadoHasta > DateTime.UtcNow)
            return Result.Fail<LoginResponse>($"Cuenta bloqueada temporalmente. Intente nuevamente más tarde.");

        if (!authService.VerifyPassword(request.Password, usuario.PasswordHash))
        {
            usuario.IntentosFallidos++;
            if (usuario.IntentosFallidos >= 5)
            {
                usuario.BloqueadoHasta = DateTime.UtcNow.AddMinutes(15 * Math.Pow(2, usuario.IntentosFallidos - 5));
                logger.LogWarning("Cuenta bloqueada por intentos fallidos: {Email}", usuario.Email);
            }
            await db.SaveChangesAsync(ct);
            return Result.Fail<LoginResponse>("Credenciales inválidas.");
        }

        usuario.IntentosFallidos = 0;
        usuario.BloqueadoHasta = null;
        usuario.UltimoAcceso = DateTime.UtcNow;

        var (accessToken, expiresAt) = authService.GenerateAccessToken(usuario);
        var refreshTokenValue = authService.GenerateRefreshToken();

        var refreshToken = new Domain.Entities.RefreshToken
        {
            UsuarioId = usuario.Id,
            TokenHash = authService.HashToken(refreshTokenValue),
            Expiracion = DateTime.UtcNow.AddDays(7)
        };

        await db.RefreshTokens.AddAsync(refreshToken, ct);
        await db.SaveChangesAsync(ct);

        return Result.Ok(new LoginResponse(
            accessToken, refreshTokenValue, expiresAt,
            usuario.NombreCompleto, usuario.Rol.Nombre, usuario.EmpresaId));
    }
}
