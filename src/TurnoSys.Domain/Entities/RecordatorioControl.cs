using TurnoSys.Domain.Entities.Common;
using TurnoSys.Domain.Enums;

namespace TurnoSys.Domain.Entities;

public class RecordatorioControl : BaseEntity
{
    public Guid TurnoOrigenId { get; set; }
    public Guid ProfesionalId { get; set; }
    public Guid PacienteId { get; set; }
    public DateOnly FechaControlSugerida { get; set; }
    public DateOnly FechaRecordatorioEnviar { get; set; }
    public EstadoRecordatorio Estado { get; set; } = EstadoRecordatorio.Pendiente;
    public DateTime? FechaEnvioReal { get; set; }
    public int IntentoEnvio { get; set; } = 0;

    public Turno TurnoOrigen { get; set; } = null!;
    public Profesional Profesional { get; set; } = null!;
    public Paciente Paciente { get; set; } = null!;
}
