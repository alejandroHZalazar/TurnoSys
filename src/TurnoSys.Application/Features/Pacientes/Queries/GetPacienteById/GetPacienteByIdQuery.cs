using MediatR;

namespace TurnoSys.Application.Features.Pacientes.Queries.GetPacienteById;

public record GetPacienteByIdQuery(Guid Id, Guid EmpresaId) : IRequest<PacienteDetalleDto?>;

public record PacienteDetalleDto(
    Guid Id,
    string Nombre,
    string Apellido,
    string NombreCompleto,
    string? DNI,
    DateOnly? FechaNacimiento,
    int? Edad,
    string? Telefono,
    string? Email,
    string? Direccion,
    string? ObraSocial,
    string? NumeroAfiliado,
    string? ContactoEmergenciaNombre,
    string? ContactoEmergenciaTelefono,
    string? Observaciones,
    string? Restricciones,
    bool ConsentimientoFirmado,
    DateTime? FechaConsentimiento,
    bool IsActivo,
    DateTime CreatedAt
);
