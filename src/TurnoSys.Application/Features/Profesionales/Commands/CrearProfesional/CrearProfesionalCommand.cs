using MediatR;
using TurnoSys.Application.Common.Models;

namespace TurnoSys.Application.Features.Profesionales.Commands.CrearProfesional;

public record CrearProfesionalCommand(
    Guid EmpresaId,
    string Nombre,
    string Apellido,
    string? Email,
    string? Telefono,
    string? Especialidad,
    string? Matricula,
    string ColorAgenda,
    string? Observaciones,
    List<HorarioDto> Horarios
) : IRequest<Result<Guid>>;

public record HorarioDto(
    int DiaSemana,   // 0=Dom, 1=Lun, ..., 6=Sab
    TimeOnly HoraInicio,
    TimeOnly HoraFin
);
