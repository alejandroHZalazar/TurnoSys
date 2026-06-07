using MediatR;
using Microsoft.EntityFrameworkCore;
using TurnoSys.Application.Common.Interfaces;
using TurnoSys.Domain.Enums;

namespace TurnoSys.Application.Features.Estadisticas.Queries.GetDashboardKPIs;

public class GetDashboardKPIsQueryHandler(IApplicationDbContext db)
    : IRequestHandler<GetDashboardKPIsQuery, DashboardKPIsDto>
{
    public async Task<DashboardKPIsDto> Handle(GetDashboardKPIsQuery request, CancellationToken ct)
    {
        var turnos = await db.Turnos
            .AsNoTracking()
            .Where(t =>
                t.EmpresaId == request.EmpresaId &&
                t.FechaHoraInicio >= request.Desde &&
                t.FechaHoraInicio <= request.Hasta &&
                !t.IsDeleted)
            .Select(t => new { t.Estado, Precio = t.Practica.Precio })
            .ToListAsync(ct);

        var total      = turnos.Count;
        var atendidos  = turnos.Count(t => t.Estado == EstadoTurno.Atendido);
        var cancelados = turnos.Count(t => t.Estado == EstadoTurno.Cancelado);
        var reservados = turnos.Count(t => t.Estado == EstadoTurno.Reservado);
        var ingresos   = turnos
            .Where(t => t.Estado == EstadoTurno.Atendido || t.Estado == EstadoTurno.Reservado)
            .Sum(t => t.Precio);
        var ocupacion  = total == 0 ? 0.0
            : Math.Round((atendidos + reservados) * 100.0 / total, 1);

        var pacientesNuevos = await db.Pacientes
            .AsNoTracking()
            .CountAsync(p =>
                p.EmpresaId == request.EmpresaId &&
                p.CreatedAt >= request.Desde &&
                p.CreatedAt <= request.Hasta, ct);

        return new DashboardKPIsDto(total, atendidos, cancelados, reservados,
            ingresos, ocupacion, pacientesNuevos);
    }
}
