using TurnoSys.Domain.Entities;

namespace TurnoSys.Domain.Interfaces.Services;

public interface IAuthService
{
    string HashPassword(string password);
    bool VerifyPassword(string password, string hash);
    (string token, DateTime expiresAt) GenerateAccessToken(Usuario usuario);
    string GenerateRefreshToken();
    string HashToken(string token);
    bool VerifyToken(string token, string hash);
}
