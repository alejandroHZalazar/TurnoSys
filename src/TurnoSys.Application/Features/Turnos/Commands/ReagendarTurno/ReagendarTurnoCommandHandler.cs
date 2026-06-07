using MediatR;
using Microsoft.EntityFrameworkCore;
using TurnoSys.Application.Common.Exceptions;
using TurnoSys.Application.Common.Interfaces;
using TurnoSys.Application.Common.Models;
using TurnoSys.Domain.Entities;
using TurnoSys.Domain.Enums;

namespace TurnoSys.Application.Features.Turnos.Commands.ReagendarTurno;

public class ReagendarTurnoCommandHandler(IApplicationDbContext db)
    : IRequestHandler<ReagendarTurnoCommand, Result>
{
    public async Task<Result> Handle(ReagendarTurnoCommand request, CancellationToken ct)
    {
        var turno = await db.Turnos
            .Include(t => t.Practica)
            .FirstOrDefaultAsync(t => t.Id == request.TurnoId && t.EmpresaId == request.EmpresaId && !t.IsDeleted, ct)
            ?? throw new NotFoundException(nameof(Turno), request.TurnoId);

        var nuevaFin = request.NuevaFechaHoraInicio.AddMinutes(turno.Practica.DuracionMinutos);

        var solapado = await db.Turnos.AnyAsync(t =>
            t.ProfesionalId == turno.ProfesionalId
            && t.Id != turno.Id
            && t.Estado != EstadoTurno.Cancelado
            && !t.IsDeleted
            && t.FechaHoraInicio < nuevaFin
            && t.FechaHoraFin > request.NuevaFechaHoraInicio, ct);

        if (solapado)
            return Result.Fail("El nuevo horario se solapa con otro turno existente.");

        turno.Reagendar(request.NuevaFechaHoraInicio, nuevaFin);
        await db.SaveChangesAsync(ct);
        return Result.Ok();
    }
}
