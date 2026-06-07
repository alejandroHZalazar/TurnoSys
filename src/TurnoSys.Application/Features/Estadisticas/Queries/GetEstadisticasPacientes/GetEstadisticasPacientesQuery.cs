using MediatR;

namespace TurnoSys.Application.Features.Estadisticas.Queries.GetEstadisticasPacientes;

public record GetEstadisticasPacientesQuery(
    Guid EmpresaId,
    DateTime Desde,
    DateTime Hasta
) : IRequest<EstadisticasPacientesDto>;

public record EstadisticasPacientesDto(
    int TotalPacientes,
    int PacientesNuevos,
    int PacientesRecurrentes,
    IEnumerable<PacienteFrecuenteDto> TopPacientes
);

public record PacienteFrecuenteDto(
    Guid PacienteId,
    string Nombre,
    int TotalTurnos,
    int TurnosAtendidos
);
