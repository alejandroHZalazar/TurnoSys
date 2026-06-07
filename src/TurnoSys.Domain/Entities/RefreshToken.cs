using TurnoSys.Domain.Entities.Common;

namespace TurnoSys.Domain.Entities;

public class RefreshToken : BaseEntity
{
    public Guid UsuarioId { get; set; }
    public string TokenHash { get; set; } = string.Empty;
    public DateTime Expiracion { get; set; }
    public bool IsRevocado { get; set; } = false;
    public DateTime? FechaRevocacion { get; set; }
    public string? IpOrigen { get; set; }
    public string? UserAgent { get; set; }

    public Usuario Usuario { get; set; } = null!;
}
