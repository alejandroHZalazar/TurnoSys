using MediatR;
using TurnoSys.Application.Common.Models;

namespace TurnoSys.Application.Features.Empresa.Commands.SubirLogo;

public record SubirLogoCommand(
    Guid EmpresaId,
    byte[] Contenido,
    string ContentType
) : IRequest<Result>;
