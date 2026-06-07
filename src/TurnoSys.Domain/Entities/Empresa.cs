using TurnoSys.Domain.Entities.Common;

namespace TurnoSys.Domain.Entities;

public class Empresa : BaseEntity
{
    public string RazonSocial { get; set; } = string.Empty;
    public string? NombreFantasia { get; set; }
    public string? CUIT { get; set; }
    public string? Direccion { get; set; }
    public string? Telefono { get; set; }
    public string? Email { get; set; }
    public string? LogotipoUrl { get; set; }
    public byte[]? LogotipoBlob { get; set; }
    public string? LogotipoContentType { get; set; }
    public string? SitioWeb { get; set; }
    public string? Instagram { get; set; }
    public string? Facebook { get; set; }
    public string? WhatsApp { get; set; }
    public TimeOnly? HorarioDesde { get; set; }
    public TimeOnly? HorarioHasta { get; set; }
    public string TimeZone { get; set; } = "America/Argentina/Buenos_Aires";
    public string? Observaciones { get; set; }
    public bool IsActivo { get; set; } = true;

    public ICollection<Profesional> Profesionales { get; set; } = [];
    public ICollection<Paciente> Pacientes { get; set; } = [];
    public ICollection<Practica> Practicas { get; set; } = [];
    public ICollection<Usuario> Usuarios { get; set; } = [];
    public ICollection<ParametroSistema> Parametros { get; set; } = [];
}
