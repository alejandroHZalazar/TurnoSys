using MediatR;
using TurnoSys.Application.Common.Models;

namespace TurnoSys.Application.Features.Turnos.Commands.ReagendarTurno;

public record ReagendarTurnoCommand(
    Guid TurnoId,
    Guid EmpresaId,
    DateTime NuevaFechaHoraInicio
) : IRequest<Result>;
