using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using TurnoSys.Domain.Interfaces.Services;

namespace TurnoSys.Infrastructure.Services.Auth;

public class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
{
    private ClaimsPrincipal? User => httpContextAccessor.HttpContext?.User;

    public Guid? UserId =>
        Guid.TryParse(User?.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : null;

    public Guid? EmpresaId =>
        Guid.TryParse(User?.FindFirstValue("empresa_id"), out var id) ? id : null;

    public string? Email => User?.FindFirstValue(ClaimTypes.Email);
    public string? Rol => User?.FindFirstValue(ClaimTypes.Role);
    public bool IsSuperAdmin => Rol == "SuperAdmin";
}
