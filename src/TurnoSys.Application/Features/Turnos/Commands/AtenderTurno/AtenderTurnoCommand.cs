using MediatR;
using TurnoSys.Application.Common.Models;

namespace TurnoSys.Application.Features.Turnos.Commands.AtenderTurno;

public record AtenderTurnoCommand(Guid TurnoId, Guid EmpresaId) : IRequest<Result>;
