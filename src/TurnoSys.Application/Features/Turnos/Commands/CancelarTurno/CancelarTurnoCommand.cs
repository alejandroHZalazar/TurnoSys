using MediatR;
using TurnoSys.Application.Common.Models;

namespace TurnoSys.Application.Features.Turnos.Commands.CancelarTurno;

public record CancelarTurnoCommand(Guid TurnoId, Guid EmpresaId, string? Motivo) : IRequest<Result>;
