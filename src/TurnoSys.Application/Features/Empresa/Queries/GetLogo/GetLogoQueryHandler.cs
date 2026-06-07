using MediatR;
using Microsoft.EntityFrameworkCore;
using TurnoSys.Application.Common.Interfaces;

namespace TurnoSys.Application.Features.Empresa.Queries.GetLogo;

public class GetLogoQueryHandler(IApplicationDbContext db)
    : IRequestHandler<GetLogoQuery, LogoResult?>
{
    public async Task<LogoResult?> Handle(GetLogoQuery request, CancellationToken ct)
    {
        var logo = await db.Empresas
            .AsNoTracking()
            .Where(e => e.Id == request.EmpresaId && e.LogotipoBlob != null)
            .Select(e => new { e.LogotipoBlob, e.LogotipoContentType })
            .FirstOrDefaultAsync(ct);

        if (logo?.LogotipoBlob is null) return null;

        return new LogoResult(
            logo.LogotipoBlob,
            logo.LogotipoContentType ?? "image/png");
    }
}
