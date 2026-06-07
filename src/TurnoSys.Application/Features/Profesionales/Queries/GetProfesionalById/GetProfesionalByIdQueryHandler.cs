using MediatR;
using Microsoft.EntityFrameworkCore;
using TurnoSys.Application.Common.Interfaces;
using TurnoSys.Application.Features.Profesionales.Queries.GetProfesionales;

namespace TurnoSys.Application.Features.Profesionales.Queries.GetProfesionalById;

public class GetProfesionalByIdQueryHandler(IApplicationDbContext db)
    : IRequestHandler<GetProfesionalByIdQuery, ProfesionalDetalleDto?>
{
    private static readonly string[] DiasSemana = ["Domingo", "Lunes", "Martes", "Miércoles", "Jueves", "Viernes", "Sábado"];

    public async Task<ProfesionalDetalleDto?> Handle(GetProfesionalByIdQuery request, CancellationToken ct)
    {
        var p = await db.Profesionales
            .AsNoTracking()
            .Include(x => x.Horarios.Where(h => h.IsActivo && !h.IsDeleted))
            .FirstOrDefaultAsync(x => x.Id == request.Id && x.EmpresaId == request.EmpresaId, ct);

        if (p is null) return null;

        return new ProfesionalDetalleDto(
            p.Id, p.Nombre, p.Apellido,
            p.Apellido + ", " + p.Nombre,
            p.Email, p.Telefono, p.Especialidad, p.Matricula,
            p.ColorAgenda, p.FotoUrl, p.Observaciones, p.IsActivo, p.CreatedAt,
            p.Horarios
                .OrderBy(h => h.DiaSemana)
                .Select(h => new HorarioListDto(
                    (int)h.DiaSemana,
                    DiasSemana[(int)h.DiaSemana],
                    h.HoraInicio.ToString("HH:mm"),
                    h.HoraFin.ToString("HH:mm")
                ))
        );
    }
}
