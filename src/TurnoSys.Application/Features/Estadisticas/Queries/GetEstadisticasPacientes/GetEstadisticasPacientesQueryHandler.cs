using MediatR;
using Microsoft.EntityFrameworkCore;
using TurnoSys.Application.Common.Interfaces;
using TurnoSys.Domain.Enums;

namespace TurnoSys.Application.Features.Estadisticas.Queries.GetEstadisticasPacientes;

public class GetEstadisticasPacientesQueryHandler(IApplicationDbContext db)
    : IRequestHandler<GetEstadisticasPacientesQuery, EstadisticasPacientesDto>
{
    public async Task<EstadisticasPacientesDto> Handle(GetEstadisticasPacientesQuery request, CancellationToken ct)
    {
        var turnos = await db.Turnos
            .AsNoTracking()
            .Where(t =>
                t.EmpresaId == request.EmpresaId &&
                t.FechaHoraInicio >= request.Desde &&
                t.FechaHoraInicio <= request.Hasta &&
                !t.IsDeleted)
            .Select(t => new
            {
                t.PacienteId,
                Nombre = t.Paciente.Apellido + ", " + t.Paciente.Nombre,
                t.Estado
            })
            .ToListAsync(ct);

        var totalPacientes = await db.Pacientes
            .CountAsync(p => p.EmpresaId == request.EmpresaId, ct);

        var nuevos = await db.Pacientes
            .CountAsync(p =>
                p.EmpresaId == request.EmpresaId &&
                p.CreatedAt >= request.Desde &&
                p.CreatedAt <= request.Hasta, ct);

        var agrupados   = turnos.GroupBy(t => new { t.PacienteId, t.Nombre }).ToList();
        var recurrentes = agrupados.Count(g => g.Count() > 1);

        var top = agrupados
            .Select(g => new PacienteFrecuenteDto(
                g.Key.PacienteId,
                g.Key.Nombre,
                g.Count(),
                g.Count(t => t.Estado == EstadoTurno.Atendido)
            ))
            .OrderByDescending(x => x.TotalTurnos)
            .Take(10);

        return new EstadisticasPacientesDto(totalPacientes, nuevos, recurrentes, top);
    }
}
