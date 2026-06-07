using MediatR;
using TurnoSys.Application.Common.Interfaces;
using TurnoSys.Application.Common.Models;
using TurnoSys.Domain.Entities;

namespace TurnoSys.Application.Features.Practicas.Commands.CrearPractica;

public class CrearPracticaCommandHandler(IApplicationDbContext db)
    : IRequestHandler<CrearPracticaCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CrearPracticaCommand request, CancellationToken ct)
    {
        var practica = new Practica
        {
            EmpresaId             = request.EmpresaId,
            CategoriaId           = request.CategoriaId,
            Nombre                = request.Nombre.Trim(),
            Descripcion           = request.Descripcion?.Trim(),
            Precio                = request.Precio,
            DuracionMinutos       = request.DuracionMinutos,
            Color                 = request.Color,
            RequiereObservaciones = request.RequiereObservaciones,
            RecordatorioRecDias   = request.RecordatorioRecDias,
            IsActivo              = true
        };

        await db.Practicas.AddAsync(practica, ct);
        await db.SaveChangesAsync(ct);
        return Result.Ok(practica.Id);
    }
}
