using MediatR;
using TurnoSys.Application.Common.Models;

namespace TurnoSys.Application.Features.Usuarios.Commands.ResetPassword;

public record ResetPasswordCommand(Guid UsuarioId, string NuevaPassword) : IRequest<Result>;
