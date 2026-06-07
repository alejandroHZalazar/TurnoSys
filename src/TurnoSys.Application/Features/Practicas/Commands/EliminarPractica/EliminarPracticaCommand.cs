using MediatR;
using TurnoSys.Application.Common.Models;

namespace TurnoSys.Application.Features.Practicas.Commands.EliminarPractica;

public record EliminarPracticaCommand(Guid Id, Guid EmpresaId) : IRequest<Result>;
