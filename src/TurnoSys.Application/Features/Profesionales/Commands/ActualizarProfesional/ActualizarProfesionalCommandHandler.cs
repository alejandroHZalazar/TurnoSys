using MediatR;
using Microsoft.EntityFrameworkCore;
using TurnoSys.Application.Common.Exceptions;
using TurnoSys.Application.Common.Interfaces;
using TurnoSys.Application.Common.Models;
using TurnoSys.Domain.Entities;

namespace TurnoSys.Application.Features.Profesionales.Commands.ActualizarProfesional;

public class ActualizarProfesionalCommandHandler(IApplicationDbContext db)
    : IRequestHandler<ActualizarProfesionalCommand, Result>
{
    public async Task<Result> Handle(ActualizarProfesionalCommand request, CancellationToken ct)
    {
        var profesional = await db.Profesionales
            .Include(p => p.Horarios)
            .FirstOrDefaultAsync(p => p.Id == request.Id && p.EmpresaId == request.EmpresaId, ct)
            ?? throw new NotFoundException(nameof(Profesional), request.Id);

        profesional.Nombre        = request.Nombre.Trim();
        profesional.Apellido      = request.Apellido.Trim();
        profesional.Email         = request.Email?.Trim().ToLower();
        profesional.Telefono      = request.Telefono?.Trim();
        profesional.Especialidad  = request.Especialidad?.Trim();
        profesional.Matricula     = request.Matricula?.Trim();
        profesional.ColorAgenda   = request.ColorAgenda;
        profesional.Observaciones = request.Observaciones?.Trim();
        profesional.IsActivo      = request.IsActivo;
        profesional.UpdatedAt     = DateTime.UtcNow;

        // Reemplazar horarios: eliminar todos y reinsertar
        db.HorariosProfesionales.RemoveRange(profesional.Horarios);
        foreach (var h in request.Horarios)
        {
            db.HorariosProfesionales.Add(new HorarioProfesional
            {
                ProfesionalId = profesional.Id,
                DiaSemana     = (DayOfWeek)h.DiaSemana,
                HoraInicio    = h.HoraInicio,
                HoraFin       = h.HoraFin,
                IsActivo      = true
            });
        }

        await db.SaveChangesAsync(ct);
        return Result.Ok();
    }
}
