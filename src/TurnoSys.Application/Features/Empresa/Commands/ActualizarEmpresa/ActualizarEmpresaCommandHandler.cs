using MediatR;
using Microsoft.EntityFrameworkCore;
using TurnoSys.Application.Common.Exceptions;
using TurnoSys.Application.Common.Interfaces;
using TurnoSys.Application.Common.Models;

namespace TurnoSys.Application.Features.Empresa.Commands.ActualizarEmpresa;

public class ActualizarEmpresaCommandHandler(IApplicationDbContext db)
    : IRequestHandler<ActualizarEmpresaCommand, Result>
{
    public async Task<Result> Handle(ActualizarEmpresaCommand request, CancellationToken ct)
    {
        var empresa = await db.Empresas
            .FirstOrDefaultAsync(e => e.Id == request.EmpresaId, ct)
            ?? throw new NotFoundException(nameof(Domain.Entities.Empresa), request.EmpresaId);

        empresa.RazonSocial    = request.RazonSocial.Trim();
        empresa.NombreFantasia = request.NombreFantasia?.Trim();
        empresa.CUIT           = request.CUIT?.Trim();
        empresa.Direccion      = request.Direccion?.Trim();
        empresa.Telefono       = request.Telefono?.Trim();
        empresa.Email          = request.Email?.Trim().ToLower();
        empresa.LogotipoUrl    = request.LogotipoUrl?.Trim();
        empresa.SitioWeb       = request.SitioWeb?.Trim();
        empresa.Instagram      = request.Instagram?.Trim();
        empresa.Facebook       = request.Facebook?.Trim();
        empresa.WhatsApp       = request.WhatsApp?.Trim();
        empresa.HorarioDesde   = ParseTime(request.HorarioDesde);
        empresa.HorarioHasta   = ParseTime(request.HorarioHasta);
        empresa.Observaciones  = request.Observaciones?.Trim();
        empresa.UpdatedAt      = DateTime.UtcNow;

        if (!string.IsNullOrWhiteSpace(request.TimeZone))
            empresa.TimeZone = request.TimeZone.Trim();

        await db.SaveChangesAsync(ct);
        return Result.Ok();
    }

    private static TimeOnly? ParseTime(string? value) =>
        TimeOnly.TryParse(value, out var t) ? t : null;
}
