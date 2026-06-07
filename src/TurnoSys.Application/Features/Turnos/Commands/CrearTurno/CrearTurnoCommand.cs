using MediatR;
using TurnoSys.Application.Common.Models;

namespace TurnoSys.Application.Features.Turnos.Commands.CrearTurno;

public record CrearTurnoCommand(
    Guid EmpresaId,
    Guid ProfesionalId,
    Guid PacienteId,
    Guid PracticaId,
    DateTime FechaHoraInicio,
    string? Observaciones,
    DateOnly? ProximoControlFecha
) : IRequest<Result<Guid>>;
