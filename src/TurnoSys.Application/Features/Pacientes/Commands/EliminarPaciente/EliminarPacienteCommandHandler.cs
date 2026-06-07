using MediatR;
using Microsoft.EntityFrameworkCore;
using TurnoSys.Application.Common.Exceptions;
using TurnoSys.Application.Common.Interfaces;
using TurnoSys.Application.Common.Models;
using TurnoSys.Domain.Entities;

namespace TurnoSys.Application.Features.Pacientes.Commands.EliminarPaciente;

public class EliminarPacienteCommandHandler(IApplicationDbContext db)
    : IRequestHandler<EliminarPacienteCommand, Result>
{
    public async Task<Result> Handle(EliminarPacienteCommand request, CancellationToken ct)
    {
        var paciente = await db.Pacientes
            .FirstOrDefaultAsync(p => p.Id == request.Id && p.EmpresaId == request.EmpresaId, ct)
            ?? throw new NotFoundException(nameof(Paciente), request.Id);

        // Verificar si tiene turnos futuros activos
        var tieneTurnosFuturos = await db.Turnos.AnyAsync(t =>
            t.PacienteId == request.Id &&
            t.FechaHoraInicio > DateTime.UtcNow &&
            t.Estado != Domain.Enums.EstadoTurno.Cancelado &&
            !t.IsDeleted, ct);

        if (tieneTurnosFuturos)
            return Result.Fail("No se puede eliminar un paciente con turnos futuros reservados.");

        paciente.IsDeleted = true;
        paciente.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);
        return Result.Ok();
    }
}
