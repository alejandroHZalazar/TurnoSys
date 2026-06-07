using MediatR;
using TurnoSys.Application.Common.Models;

namespace TurnoSys.Application.Features.Parametros.Commands.ActualizarConfiguracion;

public record ActualizarConfiguracionCommand(
    Guid EmpresaId,
    List<ParametroUpdate> Parametros
) : IRequest<Result>;

public record ParametroUpdate(string Clave, string? Valor);
