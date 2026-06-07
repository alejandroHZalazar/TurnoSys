using MediatR;

namespace TurnoSys.Application.Features.Estadisticas.Queries.GetTurnosPorPeriodo;

public record GetTurnosPorPeriodoQuery(
    Guid EmpresaId,
    DateTime Desde,
    DateTime Hasta,
    Guid? ProfesionalId = null
) : IRequest<IEnumerable<TurnosPorDiaDto>>;

public record TurnosPorDiaDto(
    string Fecha,          // "yyyy-MM-dd"
    int Total,
    int Atendidos,
    int Cancelados,
    int Reservados,
    decimal IngresoEstimado
);
