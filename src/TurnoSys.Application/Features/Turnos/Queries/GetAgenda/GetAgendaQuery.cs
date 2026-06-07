using MediatR;

namespace TurnoSys.Application.Features.Turnos.Queries.GetAgenda;

public record GetAgendaQuery(
    Guid EmpresaId,
    DateTime Desde,
    DateTime Hasta,
    Guid? ProfesionalId = null
) : IRequest<IEnumerable<TurnoAgendaDto>>;

public record TurnoAgendaDto(
    Guid Id,
    string Titulo,
    DateTime Inicio,
    DateTime Fin,
    string Color,
    string Estado,
    int EstadoId,
    Guid ProfesionalId,
    string ProfesionalNombre,
    string ProfesionalColor,
    Guid PacienteId,
    string PacienteNombre,
    string? PacienteTelefono,
    Guid PracticaId,
    string PracticaNombre,
    int DuracionMinutos,
    string? Observaciones,
    DateOnly? ProximoControlFecha
);
