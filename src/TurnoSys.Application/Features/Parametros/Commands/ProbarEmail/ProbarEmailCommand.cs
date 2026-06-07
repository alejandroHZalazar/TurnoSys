using MediatR;
using TurnoSys.Application.Common.Models;

namespace TurnoSys.Application.Features.Parametros.Commands.ProbarEmail;

public record ProbarEmailCommand(string Destinatario) : IRequest<Result>;
