using MediatR;
using Microsoft.EntityFrameworkCore;
using TurnoSys.Application.Common.Exceptions;
using TurnoSys.Application.Common.Interfaces;
using TurnoSys.Application.Common.Models;
using TurnoSys.Domain.Entities;
using TurnoSys.Domain.Enums;
using TurnoSys.Domain.Exceptions;

namespace TurnoSys.Application.Features.Turnos.Commands.CrearTurno;

public class CrearTurnoCommandHandler(IApplicationDbContext db)
    : IRequestHandler<CrearTurnoCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CrearTurnoCommand request, CancellationToken ct)
    {
        var practica = await db.Practicas
            .FirstOrDefaultAsync(p => p.Id == request.PracticaId && !p.IsDeleted && p.IsActivo, ct)
            ?? throw new NotFoundException(nameof(Practica), request.PracticaId);

        var profesional = await db.Profesionales
            .FirstOrDefaultAsync(p => p.Id == request.ProfesionalId && !p.IsDeleted && p.IsActivo, ct)
            ?? throw new NotFoundException(nameof(Profesional), request.ProfesionalId);

        var paciente = await db.Pacientes
            .FirstOrDefaultAsync(p => p.Id == request.PacienteId && !p.IsDeleted, ct)
            ?? throw new NotFoundException(nameof(Paciente), request.PacienteId);

        var fin = request.FechaHoraInicio.AddMinutes(practica.DuracionMinutos);

        var solapado = await db.Turnos.AnyAsync(t =>
            t.ProfesionalId == request.ProfesionalId
            && t.Estado != EstadoTurno.Cancelado
            && !t.IsDeleted
            && t.FechaHoraInicio < fin
            && t.FechaHoraFin > request.FechaHoraInicio, ct);

        if (solapado)
            throw new TurnoSolapamientoException(profesional.NombreCompleto, request.FechaHoraInicio, fin);

        var turno = Turno.Crear(
            request.EmpresaId, request.ProfesionalId,
            request.PacienteId, request.PracticaId,
            request.FechaHoraInicio, fin, request.Observaciones);

        if (request.ProximoControlFecha.HasValue)
        {
            turno.ProximoControlFecha = request.ProximoControlFecha;
        }

        await db.Turnos.AddAsync(turno, ct);

        if (request.ProximoControlFecha.HasValue)
        {
            var diasAnticipacion = await ObtenerDiasAnticipacionAsync(request.EmpresaId, ct);
            var recordatorio = new RecordatorioControl
            {
                TurnoOrigenId = turno.Id,
                ProfesionalId = request.ProfesionalId,
                PacienteId = request.PacienteId,
                FechaControlSugerida = request.ProximoControlFecha.Value,
                FechaRecordatorioEnviar = request.ProximoControlFecha.Value.AddDays(-diasAnticipacion)
            };
            await db.RecordatoriosControl.AddAsync(recordatorio, ct);
        }

        await db.SaveChangesAsync(ct);
        return Result.Ok(turno.Id);
    }

    private async Task<int> ObtenerDiasAnticipacionAsync(Guid empresaId, CancellationToken ct)
    {
        var param = await db.ParametrosSistema
            .FirstOrDefaultAsync(p =>
                (p.EmpresaId == empresaId || p.IsGlobal)
                && p.Clave == "RECORDATORIO_DIAS_ANTICIPACION", ct);

        return int.TryParse(param?.Valor, out var dias) ? dias : 7;
    }
}
