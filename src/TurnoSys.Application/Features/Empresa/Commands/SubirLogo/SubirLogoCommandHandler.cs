using MediatR;
using Microsoft.EntityFrameworkCore;
using TurnoSys.Application.Common.Exceptions;
using TurnoSys.Application.Common.Interfaces;
using TurnoSys.Application.Common.Models;

namespace TurnoSys.Application.Features.Empresa.Commands.SubirLogo;

public class SubirLogoCommandHandler(IApplicationDbContext db)
    : IRequestHandler<SubirLogoCommand, Result>
{
    private static readonly string[] TiposPermitidos =
        ["image/png", "image/jpeg", "image/jpg", "image/webp", "image/gif", "image/svg+xml"];

    private const int MaxBytes = 2 * 1024 * 1024; // 2 MB

    public async Task<Result> Handle(SubirLogoCommand request, CancellationToken ct)
    {
        if (request.Contenido.Length == 0)
            return Result.Fail("El archivo está vacío.");

        if (request.Contenido.Length > MaxBytes)
            return Result.Fail("El logo no puede superar los 2 MB.");

        if (!TiposPermitidos.Contains(request.ContentType.ToLower()))
            return Result.Fail("Formato no permitido. Use PNG, JPG, WEBP, GIF o SVG.");

        var empresa = await db.Empresas
            .FirstOrDefaultAsync(e => e.Id == request.EmpresaId, ct)
            ?? throw new NotFoundException(nameof(Domain.Entities.Empresa), request.EmpresaId);

        empresa.LogotipoBlob        = request.Contenido;
        empresa.LogotipoContentType = request.ContentType.ToLower();
        empresa.UpdatedAt           = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);
        return Result.Ok();
    }
}
