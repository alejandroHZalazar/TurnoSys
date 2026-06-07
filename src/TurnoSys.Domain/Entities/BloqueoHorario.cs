using TurnoSys.Domain.Entities.Common;
using TurnoSys.Domain.Enums;

namespace TurnoSys.Domain.Entities;

public class BloqueoHorario : BaseEntity
{
    public Guid ProfesionalId { get; set; }
    public DateTime FechaDesde { get; set; }
    public DateTime FechaHasta { get; set; }
    public MotivoBloqueo Motivo { get; set; } = MotivoBloqueo.Otro;
    public string? Observaciones { get; set; }

    public Profesional Profesional { get; set; } = null!;
}
