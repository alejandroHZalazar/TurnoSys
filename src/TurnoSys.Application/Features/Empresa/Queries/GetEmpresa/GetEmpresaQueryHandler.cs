using MediatR;
using Microsoft.EntityFrameworkCore;
using TurnoSys.Application.Common.Interfaces;

namespace TurnoSys.Application.Features.Empresa.Queries.GetEmpresa;

public class GetEmpresaQueryHandler(IApplicationDbContext db)
    : IRequestHandler<GetEmpresaQuery, EmpresaDto?>
{
    public async Task<EmpresaDto?> Handle(GetEmpresaQuery request, CancellationToken ct)
    {
        var e = await db.Empresas
            .AsNoTracking()
            .Where(x => x.Id == request.EmpresaId)
            .Select(x => new
            {
                x.Id, x.RazonSocial, x.NombreFantasia, x.CUIT, x.Direccion,
                x.Telefono, x.Email, x.LogotipoUrl, x.SitioWeb,
                x.Instagram, x.Facebook, x.WhatsApp,
                x.HorarioDesde, x.HorarioHasta, x.TimeZone,
                x.Observaciones, x.IsActivo,
                TieneLogo = x.LogotipoBlob != null
            })
            .FirstOrDefaultAsync(ct);

        if (e is null) return null;

        return new EmpresaDto(
            e.Id, e.RazonSocial, e.NombreFantasia, e.CUIT, e.Direccion,
            e.Telefono, e.Email, e.LogotipoUrl, e.SitioWeb,
            e.Instagram, e.Facebook, e.WhatsApp,
            e.HorarioDesde?.ToString("HH:mm"),
            e.HorarioHasta?.ToString("HH:mm"),
            e.TimeZone, e.Observaciones, e.IsActivo, e.TieneLogo);
    }
}
