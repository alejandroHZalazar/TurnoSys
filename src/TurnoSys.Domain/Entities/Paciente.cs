using TurnoSys.Domain.Entities.Common;

namespace TurnoSys.Domain.Entities;

public class Paciente : AuditableEntity
{
    public Guid EmpresaId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Apellido { get; set; } = string.Empty;
    public string? DNI { get; set; }
    public DateOnly? FechaNacimiento { get; set; }
    public string? Telefono { get; set; }
    public string? Email { get; set; }
    public string? Direccion { get; set; }
    public string? ObraSocial { get; set; }
    public string? NumeroAfiliado { get; set; }
    public string? ContactoEmergenciaNombre { get; set; }
    public string? ContactoEmergenciaTelefono { get; set; }
    public string? Observaciones { get; set; }
    public string? Restricciones { get; set; }
    public bool ConsentimientoFirmado { get; set; } = false;
    public DateTime? FechaConsentimiento { get; set; }
    public bool IsActivo { get; set; } = true;

    public string NombreCompleto => $"{Apellido}, {Nombre}";

    public Empresa Empresa { get; set; } = null!;
    public ICollection<Turno> Turnos { get; set; } = [];
}
