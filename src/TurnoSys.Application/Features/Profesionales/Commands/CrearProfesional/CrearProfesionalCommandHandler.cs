using MediatR;
using TurnoSys.Application.Common.Interfaces;
using TurnoSys.Application.Common.Models;
using TurnoSys.Domain.Entities;

namespace TurnoSys.Application.Features.Profesionales.Commands.CrearProfesional;

public class CrearProfesionalCommandHandler(IApplicationDbContext db)
    : IRequestHandler<CrearProfesionalCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CrearProfesionalCommand request, CancellationToken ct)
    {
        var profesional = new Profesional
        {
            EmpresaId    = request.EmpresaId,
            Nombre       = request.Nombre.Trim(),
            Apellido     = request.Apellido.Trim(),
            Email        = request.Email?.Trim().ToLower(),
            Telefono     = request.Telefono?.Trim(),
            Especialidad = request.Especialidad?.Trim(),
            Matricula    = request.Matricula?.Trim(),
            ColorAgenda  = request.ColorAgenda,
            Observaciones = request.Observaciones?.Trim(),
            IsActivo     = true
        };

        foreach (var h in request.Horarios)
        {
            profesional.Horarios.Add(new HorarioProfesional
            {
                ProfesionalId = profesional.Id,
                DiaSemana     = (DayOfWeek)h.DiaSemana,
                HoraInicio    = h.HoraInicio,
                HoraFin       = h.HoraFin,
                IsActivo      = true
            });
        }

        await db.Profesionales.AddAsync(profesional, ct);
        await db.SaveChangesAsync(ct);

        return Result.Ok(profesional.Id);
    }
}
