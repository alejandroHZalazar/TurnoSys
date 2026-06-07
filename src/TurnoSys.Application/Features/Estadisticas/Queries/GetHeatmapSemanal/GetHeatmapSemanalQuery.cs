using MediatR;

namespace TurnoSys.Application.Features.Estadisticas.Queries.GetHeatmapSemanal;

public record GetHeatmapSemanalQuery(
    Guid EmpresaId,
    int DiasAtras = 90,
    Guid? ProfesionalId = null
) : IRequest<IEnumerable<HeatmapCeldaDto>>;

public record HeatmapCeldaDto(
    int DiaSemana,   // 0=Dom … 6=Sab
    int Hora,        // 0-23
    int Cantidad
);
