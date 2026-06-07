using MediatR;

namespace TurnoSys.Application.Features.Parametros.Queries.GetConfiguracion;

public record GetConfiguracionQuery(Guid EmpresaId) : IRequest<IEnumerable<SeccionConfigDto>>;

public record SeccionConfigDto(
    string Seccion,
    IEnumerable<ParametroConfigDto> Parametros
);

public record ParametroConfigDto(
    string Clave,
    string Label,
    string Tipo,
    string Valor,        // vacío si es secreto (no se expone)
    bool EsSecreto,
    bool Configurado,    // para secretos: indica si tiene valor seteado
    string? Opciones,
    string? Ayuda
);
