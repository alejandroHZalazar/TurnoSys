using MediatR;

namespace TurnoSys.Application.Features.Estadisticas.Queries.GetEstadisticasProfesionales;

public record GetEstadisticasProfesionalesQuery(
    Guid EmpresaId,
    DateTime Desde,
    DateTime Hasta
) : IRequest<IEnumerable<EstadisticaProfesionalDto>>;

public record EstadisticaProfesionalDto(
    Guid ProfesionalId,
    string Nombre,
    string ColorAgenda,
    int TotalTurnos,
    int Atendidos,
    int Cancelados,
    decimal IngresoEstimado,
    double PorcentajeOcupacion
);
