using MediatR;
using TurnoSys.Application.Common.Models;

namespace TurnoSys.Application.Features.Empresa.Commands.ActualizarEmpresa;

public record ActualizarEmpresaCommand(
    Guid EmpresaId,
    string RazonSocial,
    string? NombreFantasia,
    string? CUIT,
    string? Direccion,
    string? Telefono,
    string? Email,
    string? LogotipoUrl,
    string? SitioWeb,
    string? Instagram,
    string? Facebook,
    string? WhatsApp,
    string? HorarioDesde,   // "HH:mm"
    string? HorarioHasta,
    string? TimeZone,
    string? Observaciones
) : IRequest<Result>;
