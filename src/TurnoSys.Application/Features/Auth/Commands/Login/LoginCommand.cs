using MediatR;
using TurnoSys.Application.Common.Models;

namespace TurnoSys.Application.Features.Auth.Commands.Login;

public record LoginCommand(string Email, string Password) : IRequest<Result<LoginResponse>>;

public record LoginResponse(
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAt,
    string NombreCompleto,
    string Rol,
    Guid? EmpresaId
);
