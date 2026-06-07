using MediatR;
using TurnoSys.Application.Common.Models;

namespace TurnoSys.Application.Features.Profesionales.Commands.EliminarProfesional;

public record EliminarProfesionalCommand(Guid Id, Guid EmpresaId) : IRequest<Result>;
