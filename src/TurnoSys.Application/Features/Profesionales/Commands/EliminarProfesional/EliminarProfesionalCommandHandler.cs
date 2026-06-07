using MediatR;
using Microsoft.EntityFrameworkCore;
using TurnoSys.Application.Common.Exceptions;
using TurnoSys.Application.Common.Interfaces;
using TurnoSys.Application.Common.Models;
using TurnoSys.Domain.Entities;

namespace TurnoSys.Application.Features.Profesionales.Commands.EliminarProfesional;

public class EliminarProfesionalCommandHandler(IApplicationDbContext db)
    : IRequestHandler<EliminarProfesionalCommand, Result>
{
    public async Task<Result> Handle(EliminarProfesionalCommand request, CancellationToken ct)
    {
        var profesional = await db.Profesionales
            .FirstOrDefaultAsync(p => p.Id == request.Id && p.EmpresaId == request.EmpresaId, ct)
            ?? throw new NotFoundException(nameof(Profesional), request.Id);

        var tieneTurnosFuturos = await db.Turnos.AnyAsync(t =>
            t.ProfesionalId == request.Id &&
            t.FechaHoraInicio > DateTime.UtcNow &&
            t.Estado != Domain.Enums.EstadoTurno.Cancelado &&
            !t.IsDeleted, ct);

        if (tieneTurnosFuturos)
            return Result.Fail("No se puede eliminar un profesional con turnos futuros reservados.");

        profesional.IsDeleted = true;
        profesional.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);
        return Result.Ok();
    }
}
