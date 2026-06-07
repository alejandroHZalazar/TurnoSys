namespace TurnoSys.Domain.Interfaces.Services;

public interface ICurrentUserService
{
    Guid? UserId { get; }
    Guid? EmpresaId { get; }
    string? Email { get; }
    string? Rol { get; }
    bool IsSuperAdmin { get; }
}
