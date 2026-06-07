using MediatR;
using Microsoft.EntityFrameworkCore;
using TurnoSys.Application.Common.Interfaces;
using TurnoSys.Domain.Enums;

namespace TurnoSys.Application.Features.Estadisticas.Queries.GetHeatmapSemanal;

public class GetHeatmapSemanalQueryHandler(IApplicationDbContext db)
    : IRequestHandler<GetHeatmapSemanalQuery, IEnumerable<HeatmapCeldaDto>>
{
    public async Task<IEnumerable<HeatmapCeldaDto>> Handle(GetHeatmapSemanalQuery request, CancellationToken ct)
    {
        var desde = DateTime.UtcNow.AddDays(-request.DiasAtras);

        var query = db.Turnos
            .AsNoTracking()
            .Where(t =>
                t.EmpresaId == request.EmpresaId &&
                t.FechaHoraInicio >= desde &&
                t.Estado != EstadoTurno.Cancelado &&
                !t.IsDeleted);

        if (request.ProfesionalId.HasValue)
            query = query.Where(t => t.ProfesionalId == request.ProfesionalId.Value);

        var raw = await query
            .Select(t => new { t.FechaHoraInicio })
            .ToListAsync(ct);

        return raw
            .GroupBy(t => new { DiaSemana = (int)t.FechaHoraInicio.DayOfWeek, Hora = t.FechaHoraInicio.Hour })
            .Select(g => new HeatmapCeldaDto(g.Key.DiaSemana, g.Key.Hora, g.Count()))
            .OrderBy(x => x.DiaSemana).ThenBy(x => x.Hora);
    }
}
