using MediatR;
using Microsoft.EntityFrameworkCore;
using TurnoSys.Application.Common.Exceptions;
using TurnoSys.Application.Common.Interfaces;
using TurnoSys.Application.Common.Models;
using TurnoSys.Domain.Entities;

namespace TurnoSys.Application.Features.Practicas.Commands.ActualizarPractica;

public class ActualizarPracticaCommandHandler(IApplicationDbContext db)
    : IRequestHandler<ActualizarPracticaCommand, Result>
{
    public async Task<Result> Handle(ActualizarPracticaCommand request, CancellationToken ct)
    {
        var practica = await db.Practicas
            .FirstOrDefaultAsync(p => p.Id == request.Id && p.EmpresaId == request.EmpresaId, ct)
            ?? throw new NotFoundException(nameof(Practica), request.Id);

        practica.CategoriaId           = request.CategoriaId;
        practica.Nombre                = request.Nombre.Trim();
        practica.Descripcion           = request.Descripcion?.Trim();
        practica.Precio                = request.Precio;
        practica.DuracionMinutos       = request.DuracionMinutos;
        practica.Color                 = request.Color;
        practica.RequiereObservaciones = request.RequiereObservaciones;
        practica.RecordatorioRecDias   = request.RecordatorioRecDias;
        practica.IsActivo              = request.IsActivo;
        practica.UpdatedAt             = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);
        return Result.Ok();
    }
}
