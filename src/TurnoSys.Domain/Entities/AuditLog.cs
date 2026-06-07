namespace TurnoSys.Domain.Entities;

public class AuditLog
{
    public long Id { get; set; }
    public Guid? EmpresaId { get; set; }
    public Guid? UsuarioId { get; set; }
    public string Accion { get; set; } = string.Empty;
    public string Entidad { get; set; } = string.Empty;
    public string? EntidadId { get; set; }
    public string? ValoresAnteriores { get; set; }
    public string? ValoresNuevos { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public DateTime FechaHora { get; set; } = DateTime.UtcNow;
}
