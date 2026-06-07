using TurnoSys.Domain.Entities.Common;
using TurnoSys.Domain.Enums;
using TurnoSys.Domain.Exceptions;

namespace TurnoSys.Domain.Entities;

public class Turno : AuditableEntity
{
    public Guid EmpresaId { get; set; }
    public Guid ProfesionalId { get; set; }
    public Guid PacienteId { get; set; }
    public Guid PracticaId { get; set; }
    public DateTime FechaHoraInicio { get; private set; }
    public DateTime FechaHoraFin { get; private set; }
    public EstadoTurno Estado { get; private set; } = EstadoTurno.Reservado;
    public string? Observaciones { get; set; }
    public string? MotivoCancelacion { get; private set; }
    public DateOnly? ProximoControlFecha { get; set; }
    public bool RecordatorioTurnoEnviado { get; set; } = false;
    public bool RecordatorioControlEnviado { get; set; } = false;

    public Empresa Empresa { get; set; } = null!;
    public Profesional Profesional { get; set; } = null!;
    public Paciente Paciente { get; set; } = null!;
    public Practica Practica { get; set; } = null!;
    public RecordatorioControl? RecordatorioControl { get; set; }

    protected Turno() { }

    public static Turno Crear(
        Guid empresaId, Guid profesionalId, Guid pacienteId, Guid practicaId,
        DateTime inicio, DateTime fin, string? observaciones = null)
    {
        if (fin <= inicio)
            throw new DomainException("La hora de fin debe ser posterior a la de inicio.");

        return new Turno
        {
            EmpresaId = empresaId,
            ProfesionalId = profesionalId,
            PacienteId = pacienteId,
            PracticaId = practicaId,
            FechaHoraInicio = inicio,
            FechaHoraFin = fin,
            Estado = EstadoTurno.Reservado,
            Observaciones = observaciones
        };
    }

    public void Cancelar(string? motivo)
    {
        if (Estado == EstadoTurno.Atendido)
            throw new DomainException("No se puede cancelar un turno ya atendido.");

        Estado = EstadoTurno.Cancelado;
        MotivoCancelacion = motivo;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Reagendar(DateTime nuevaInicio, DateTime nuevaFin)
    {
        if (Estado == EstadoTurno.Cancelado || Estado == EstadoTurno.Atendido)
            throw new DomainException("No se puede reagendar un turno cancelado o atendido.");

        if (nuevaFin <= nuevaInicio)
            throw new DomainException("La hora de fin debe ser posterior a la de inicio.");

        FechaHoraInicio = nuevaInicio;
        FechaHoraFin = nuevaFin;
        RecordatorioTurnoEnviado = false;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarcarAtendido()
    {
        if (Estado != EstadoTurno.Reservado)
            throw new DomainException("Solo se pueden atender turnos en estado Reservado.");

        Estado = EstadoTurno.Atendido;
        UpdatedAt = DateTime.UtcNow;
    }
}
