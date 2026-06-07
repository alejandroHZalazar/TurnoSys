using TurnoSys.Domain.Entities.Common;

namespace TurnoSys.Domain.Entities;

public class HorarioProfesional : BaseEntity
{
    public Guid ProfesionalId { get; set; }
    public DayOfWeek DiaSemana { get; set; }
    public TimeOnly HoraInicio { get; set; }
    public TimeOnly HoraFin { get; set; }
    public bool IsActivo { get; set; } = true;
    public DateOnly? VigenciaDesde { get; set; }
    public DateOnly? VigenciaHasta { get; set; }

    public Profesional Profesional { get; set; } = null!;
}
