using MediatR;
using TurnoSys.Application.Common.Models;

namespace TurnoSys.Application.Features.Pacientes.Queries.GetPacientes;

public record GetPacientesQuery(
    Guid EmpresaId,
    string? Busqueda = null,   // busca por nombre, apellido o DNI
    bool? SoloActivos = true,
    int Page = 1,
    int PageSize = 20
) : IRequest<PagedResult<PacienteListDto>>;

public record PacienteListDto(
    Guid Id,
    string Nombre,
    string Apellido,
    string NombreCompleto,
    string? DNI,
    string? Telefono,
    string? Email,
    string? ObraSocial,
    int? Edad,
    bool IsActivo,
    int TotalTurnos
);
