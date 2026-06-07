using MediatR;
using TurnoSys.Application.Features.Profesionales.Queries.GetProfesionales;

namespace TurnoSys.Application.Features.Profesionales.Queries.GetProfesionalById;

public record GetProfesionalByIdQuery(Guid Id, Guid EmpresaId) : IRequest<ProfesionalDetalleDto?>;

public record ProfesionalDetalleDto(
    Guid Id,
    string Nombre,
    string Apellido,
    string NombreCompleto,
    string? Email,
    string? Telefono,
    string? Especialidad,
    string? Matricula,
    string ColorAgenda,
    string? FotoUrl,
    string? Observaciones,
    bool IsActivo,
    DateTime CreatedAt,
    IEnumerable<HorarioListDto> Horarios
);
