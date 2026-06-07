using MediatR;
using Microsoft.EntityFrameworkCore;
using TurnoSys.Application.Common.Interfaces;
using TurnoSys.Domain.Enums;

namespace TurnoSys.Application.Features.Estadisticas.Queries.GetEstadisticasProfesionales;

public class GetEstadisticasProfesionalesQueryHandler(IApplicationDbContext db)
    : IRequestHandler<GetEstadisticasProfesionalesQuery, IEnumerable<EstadisticaProfesionalDto>>
{
    public async Task<IEnumerable<EstadisticaProfesionalDto>> Handle(
        GetEstadisticasProfesionalesQuery request, CancellationToken ct)
    {
        var raw = await db.Turnos
            .AsNoTracking()
            .Where(t =>
                t.EmpresaId == request.EmpresaId &&
                t.FechaHoraInicio >= request.Desde &&
                t.FechaHoraInicio <= request.Hasta &&
                !t.IsDeleted)
            .Select(t => new
            {
                t.ProfesionalId,
                NombreProfesional = t.Profesional.Apellido + ", " + t.Profesional.Nombre,
                t.Profesional.ColorAgenda,
                t.Estado,
                Precio = t.Practica.Precio
            })
            .ToListAsync(ct);

        return raw
            .GroupBy(t => new { t.ProfesionalId, t.NombreProfesional, t.ColorAgenda })
            .Select(g =>
            {
                var total     = g.Count();
                var atendidos = g.Count(t => t.Estado == EstadoTurno.Atendido);
                var ingresos  = g
                    .Where(t => t.Estado == EstadoTurno.Atendido || t.Estado == EstadoTurno.Reservado)
                    .Sum(t => t.Precio);
                return new EstadisticaProfesionalDto(
                    g.Key.ProfesionalId,
                    g.Key.NombreProfesional,
                    g.Key.ColorAgenda,
                    total,
                    atendidos,
                    g.Count(t => t.Estado == EstadoTurno.Cancelado),
                    ingresos,
                    total == 0 ? 0 : Math.Round(atendidos * 100.0 / total, 1)
                );
            })
            .OrderByDescending(x => x.TotalTurnos);
    }
}
