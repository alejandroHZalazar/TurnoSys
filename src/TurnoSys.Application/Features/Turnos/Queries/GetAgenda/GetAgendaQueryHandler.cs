using MediatR;
using Microsoft.EntityFrameworkCore;
using TurnoSys.Application.Common.Interfaces;
using TurnoSys.Domain.Enums;

namespace TurnoSys.Application.Features.Turnos.Queries.GetAgenda;

public class GetAgendaQueryHandler(IApplicationDbContext db)
    : IRequestHandler<GetAgendaQuery, IEnumerable<TurnoAgendaDto>>
{
    public async Task<IEnumerable<TurnoAgendaDto>> Handle(GetAgendaQuery request, CancellationToken ct)
    {
        return await db.Turnos
            .AsNoTracking()
            .Where(t =>
                t.EmpresaId == request.EmpresaId
                && !t.IsDeleted
                && t.Estado != Domain.Enums.EstadoTurno.Cancelado  // cancelados liberan el slot
                && t.FechaHoraInicio >= request.Desde
                && t.FechaHoraInicio <= request.Hasta
                && (!request.ProfesionalId.HasValue || t.ProfesionalId == request.ProfesionalId))
            .OrderBy(t => t.FechaHoraInicio)
            .Select(t => new TurnoAgendaDto(
                t.Id,
                t.Paciente.Apellido + ", " + t.Paciente.Nombre + " — " + t.Practica.Nombre,
                t.FechaHoraInicio,
                t.FechaHoraFin,
                ColorPorEstado(t.Estado),
                t.Estado.ToString(),
                (int)t.Estado,
                t.ProfesionalId,
                t.Profesional.Apellido + ", " + t.Profesional.Nombre,
                t.Profesional.ColorAgenda,
                t.PacienteId,
                t.Paciente.Apellido + ", " + t.Paciente.Nombre,
                t.Paciente.Telefono,
                t.PracticaId,
                t.Practica.Nombre,
                t.Practica.DuracionMinutos,
                t.Observaciones,
                t.ProximoControlFecha
            ))
            .ToListAsync(ct);
    }

    private static string ColorPorEstado(EstadoTurno estado) => estado switch
    {
        EstadoTurno.Disponible => "#22C55E",
        EstadoTurno.Reservado  => "#3B82F6",
        EstadoTurno.Cancelado  => "#EF4444",
        EstadoTurno.Atendido   => "#8B5CF6",
        _                      => "#6B7280"
    };
}
