namespace TurnoSys.Domain.Entities;

public class Rol
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    /// <summary>
    /// JSON array de permisos (ej: ["agenda.ver","pacientes.editar"]).
    /// NULL significa acceso total (SuperAdmin).
    /// </summary>
    public string? Permisos { get; set; }

    public ICollection<Usuario> Usuarios { get; set; } = [];
}
