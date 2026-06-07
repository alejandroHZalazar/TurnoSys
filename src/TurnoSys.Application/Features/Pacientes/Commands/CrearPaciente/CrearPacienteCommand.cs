using MediatR;
using TurnoSys.Application.Common.Models;

namespace TurnoSys.Application.Features.Pacientes.Commands.CrearPaciente;

public record CrearPacienteCommand(
    Guid EmpresaId,
    string Nombre,
    string Apellido,
    string? DNI,
    DateOnly? FechaNacimiento,
    string? Telefono,
    string? Email,
    string? Direccion,
    string? ObraSocial,
    string? NumeroAfiliado,
    string? ContactoEmergenciaNombre,
    string? ContactoEmergenciaTelefono,
    string? Observaciones,
    string? Restricciones
) : IRequest<Result<Guid>>;
