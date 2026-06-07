using MediatR;

namespace TurnoSys.Application.Features.Estadisticas.Queries.GetDashboardKPIs;

public record GetDashboardKPIsQuery(
    Guid EmpresaId,
    DateTime Desde,
    DateTime Hasta
) : IRequest<DashboardKPIsDto>;

public record DashboardKPIsDto(
    int TotalTurnos,
    int Atendidos,
    int Cancelados,
    int Reservados,
    decimal IngresoEstimado,
    double PorcentajeOcupacion,
    int PacientesNuevos
);
