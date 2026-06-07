using MediatR;
using Microsoft.EntityFrameworkCore;
using TurnoSys.Application.Common.Exceptions;
using TurnoSys.Application.Common.Interfaces;
using TurnoSys.Application.Common.Models;
using TurnoSys.Domain.Entities;

namespace TurnoSys.Application.Features.Turnos.Commands.CancelarTurno;

public class CancelarTurnoCommandHandler(IApplicationDbContext db)
    : IRequestHandler<CancelarTurnoCommand, Result>
{
    public async Task<Result> Handle(CancelarTurnoCommand request, CancellationToken ct)
    {
        var turno = await db.Turnos
            .FirstOrDefaultAsync(t => t.Id == request.TurnoId && t.EmpresaId == request.EmpresaId && !t.IsDeleted, ct)
            ?? throw new NotFoundException(nameof(Turno), request.TurnoId);

        turno.Cancelar(request.Motivo);
        await db.SaveChangesAsync(ct);
        return Result.Ok();
    }
}
