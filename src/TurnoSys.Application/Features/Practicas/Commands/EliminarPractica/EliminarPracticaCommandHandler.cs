using MediatR;
using Microsoft.EntityFrameworkCore;
using TurnoSys.Application.Common.Exceptions;
using TurnoSys.Application.Common.Interfaces;
using TurnoSys.Application.Common.Models;
using TurnoSys.Domain.Entities;

namespace TurnoSys.Application.Features.Practicas.Commands.EliminarPractica;

public class EliminarPracticaCommandHandler(IApplicationDbContext db)
    : IRequestHandler<EliminarPracticaCommand, Result>
{
    public async Task<Result> Handle(EliminarPracticaCommand request, CancellationToken ct)
    {
        var practica = await db.Practicas
            .FirstOrDefaultAsync(p => p.Id == request.Id && p.EmpresaId == request.EmpresaId, ct)
            ?? throw new NotFoundException(nameof(Practica), request.Id);

        var usadaEnTurnos = await db.Turnos.AnyAsync(t =>
            t.PracticaId == request.Id &&
            t.FechaHoraInicio > DateTime.UtcNow &&
            t.Estado != Domain.Enums.EstadoTurno.Cancelado &&
            !t.IsDeleted, ct);

        if (usadaEnTurnos)
            return Result.Fail("No se puede eliminar una práctica con turnos futuros reservados.");

        practica.IsDeleted = true;
        practica.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);
        return Result.Ok();
    }
}
