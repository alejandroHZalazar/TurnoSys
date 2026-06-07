using MediatR;
using TurnoSys.Application.Common.Models;

namespace TurnoSys.Application.Features.Pacientes.Commands.EliminarPaciente;

public record EliminarPacienteCommand(Guid Id, Guid EmpresaId) : IRequest<Result>;
