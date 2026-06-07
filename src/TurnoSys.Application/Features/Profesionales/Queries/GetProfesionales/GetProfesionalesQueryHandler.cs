using MediatR;
using Microsoft.EntityFrameworkCore;
using TurnoSys.Application.Common.Interfaces;

namespace TurnoSys.Application.Features.Profesionales.Queries.GetProfesionales;

public class GetProfesionalesQueryHandler(IApplicationDbContext db)
    : IRequestHandler<GetProfesionalesQuery, IEnumerable<ProfesionalListDto>>
{
    private static readonly string[] DiasSemana = ["Domingo", "Lunes", "Martes", "Miércoles", "Jueves", "Viernes", "Sábado"];

    public async Task<IEnumerable<ProfesionalListDto>> Handle(GetProfesionalesQuery request, CancellationToken ct)
    {
        var query = db.Profesionales
            .AsNoTracking()
            .Include(p => p.Horarios.Where(h => h.IsActivo && !h.IsDeleted))
            .Where(p => p.EmpresaId == request.EmpresaId);

        if (request.SoloActivos == true)
            query = query.Where(p => p.IsActivo);

        var profesionales = await query
            .OrderBy(p => p.Apellido)
            .ThenBy(p => p.Nombre)
            .ToListAsync(ct);

        return profesionales.Select(p => new ProfesionalListDto(
            p.Id,
            p.Nombre,
            p.Apellido,
            p.Apellido + ", " + p.Nombre,
            p.Especialidad,
            p.Matricula,
            p.Telefono,
            p.Email,
            p.ColorAgenda,
            p.IsActivo,
            db.Turnos.Count(t => t.ProfesionalId == p.Id && !t.IsDeleted),
            p.Horarios
                .OrderBy(h => h.DiaSemana)
                .Select(h => new HorarioListDto(
                    (int)h.DiaSemana,
                    DiasSemana[(int)h.DiaSemana],
                    h.HoraInicio.ToString("HH:mm"),
                    h.HoraFin.ToString("HH:mm")
                ))
        ));
    }
}
