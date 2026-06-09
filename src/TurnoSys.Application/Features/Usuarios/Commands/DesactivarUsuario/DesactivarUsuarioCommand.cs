using MediatR;
using TurnoSys.Application.Common.Models;

namespace TurnoSys.Application.Features.Usuarios.Commands.DesactivarUsuario;

public record DesactivarUsuarioCommand(Guid Id) : IRequest<Result>;
