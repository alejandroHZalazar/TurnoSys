using TurnoSys.Domain.Entities.Common;

namespace TurnoSys.Domain.Entities;

public class Notificacion : BaseEntity
{
    public Guid UsuarioId { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string Mensaje { get; set; } = string.Empty;
    public string Tipo { get; set; } = "info";
    public bool IsLeida { get; set; } = false;
    public string? Url { get; set; }

    public Usuario Usuario { get; set; } = null!;
}
