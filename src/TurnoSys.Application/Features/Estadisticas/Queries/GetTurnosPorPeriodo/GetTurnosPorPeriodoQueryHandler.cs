using MediatR;
using Microsoft.EntityFrameworkCore;
using TurnoSys.Application.Common.Interfaces;
using TurnoSys.Domain.Enums;

namespace TurnoSys.Application.Features.Estadisticas.Queries.GetTurnosPorPeriodo;

public class GetTurnosPorPeriodoQueryHandler(IApplicationDbContext db)
    : IRequestHandler<GetTurnosPorPeriodoQuery, IEnumerable<TurnosPorDiaDto>>
{
    public async Task<IEnumerable<TurnosPorDiaDto>> Handle(GetTurnosPorPeriodoQuery request, CancellationToken ct)
    {
        var query = db.Turnos
            .AsNoTracking()
            .Where(t =>
                t.EmpresaId == request.EmpresaId &&
                t.FechaHoraInicio >= request.Desde &&
                t.FechaHoraInicio <= request.Hasta &&
                !t.IsDeleted);

        if (request.ProfesionalId.HasValue)
            query = query.Where(t => t.ProfesionalId == request.ProfesionalId.Value);

        var raw = await query
            .Select(t => new { Fecha = t.FechaHoraInicio.Date, t.Estado, Precio = t.Practica.Precio })
            .ToListAsync(ct);

        return raw
            .GroupBy(t => t.Fecha)
            .OrderBy(g => g.Key)
            .Select(g => new TurnosPorDiaDto(
                g.Key.ToString("yyyy-MM-dd"),
                g.Count(),
                g.Count(t => t.Estado == EstadoTurno.Atendido),
                g.Count(t => t.Estado == EstadoTurno.Cancelado),
                g.Count(t => t.Estado == EstadoTurno.Reservado),
                g.Where(t => t.Estado == EstadoTurno.Atendido || t.Estado == EstadoTurno.Reservado)
                 .Sum(t => t.Precio)
            ));
    }
}
