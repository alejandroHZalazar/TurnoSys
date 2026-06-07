using MediatR;
using TurnoSys.Application.Common.Models;
using TurnoSys.Application.Features.Profesionales.Commands.CrearProfesional;

namespace TurnoSys.Application.Features.Profesionales.Commands.ActualizarProfesional;

public record ActualizarProfesionalCommand(
    Guid Id,
    Guid EmpresaId,
    string Nombre,
    string Apellido,
    string? Email,
    string? Telefono,
    string? Especialidad,
    string? Matricula,
    string ColorAgenda,
    string? Observaciones,
    bool IsActivo,
    List<HorarioDto> Horarios
) : IRequest<Result>;
