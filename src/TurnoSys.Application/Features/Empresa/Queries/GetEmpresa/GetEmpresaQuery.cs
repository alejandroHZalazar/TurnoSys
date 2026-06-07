using MediatR;

namespace TurnoSys.Application.Features.Empresa.Queries.GetEmpresa;

public record GetEmpresaQuery(Guid EmpresaId) : IRequest<EmpresaDto?>;

public record EmpresaDto(
    Guid Id,
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
    string TimeZone,
    string? Observaciones,
    bool IsActivo,
    bool TieneLogo
);
