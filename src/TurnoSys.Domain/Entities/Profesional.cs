using TurnoSys.Domain.Entities.Common;

namespace TurnoSys.Domain.Entities;

public class Profesional : AuditableEntity
{
    public Guid EmpresaId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Apellido { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Telefono { get; set; }
    public string? Especialidad { get; set; }
    public string? Matricula { get; set; }
    public string ColorAgenda { get; set; } = "#4F46E5";
    public string? FotoUrl { get; set; }
    public bool IsActivo { get; set; } = true;
    public string? Observaciones { get; set; }

    public string NombreCompleto => $"{Apellido}, {Nombre}";

    public Empresa Empresa { get; set; } = null!;
    public ICollection<HorarioProfesional> Horarios { get; set; } = [];
    public ICollection<BloqueoHorario> Bloqueos { get; set; } = [];
    public ICollection<Turno> Turnos { get; set; } = [];
    public ICollection<ProfesionalPractica> ProfesionalesPracticas { get; set; } = [];
}
