using MediatR;

namespace TurnoSys.Application.Features.Profesionales.Queries.GetProfesionales;

public record GetProfesionalesQuery(
    Guid EmpresaId,
    bool? SoloActivos = true
) : IRequest<IEnumerable<ProfesionalListDto>>;

public record ProfesionalListDto(
    Guid Id,
    string Nombre,
    string Apellido,
    string NombreCompleto,
    string? Especialidad,
    string? Matricula,
    string? Telefono,
    string? Email,
    string ColorAgenda,
    bool IsActivo,
    int TotalTurnos,
    IEnumerable<HorarioListDto> Horarios
);

public record HorarioListDto(
    int DiaSemana,
    string DiaNombre,
    string HoraInicio,
    string HoraFin
);
