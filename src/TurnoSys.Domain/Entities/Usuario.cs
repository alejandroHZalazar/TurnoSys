using TurnoSys.Domain.Entities.Common;

namespace TurnoSys.Domain.Entities;

public class Usuario : BaseEntity
{
    public Guid? EmpresaId { get; set; }
    public int RolId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string NombreCompleto { get; set; } = string.Empty;
    public bool IsActivo { get; set; } = true;
    public DateTime? UltimoAcceso { get; set; }
    public int IntentosFallidos { get; set; } = 0;
    public DateTime? BloqueadoHasta { get; set; }

    public Guid? ProfesionalId { get; set; }

    public Empresa? Empresa { get; set; }
    public Rol Rol { get; set; } = null!;
    public Profesional? Profesional { get; set; }
    public ICollection<RefreshToken> RefreshTokens { get; set; } = [];
    public ICollection<Notificacion> Notificaciones { get; set; } = [];
}
