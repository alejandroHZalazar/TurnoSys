using MediatR;
using Microsoft.EntityFrameworkCore;
using TurnoSys.Application.Common.Interfaces;
using TurnoSys.Application.Common.Models;
using TurnoSys.Domain.Entities;

namespace TurnoSys.Application.Features.Parametros.Commands.ActualizarConfiguracion;

public class ActualizarConfiguracionCommandHandler(
    IApplicationDbContext db,
    IRecordatorioScheduler scheduler)
    : IRequestHandler<ActualizarConfiguracionCommand, Result>
{
    public async Task<Result> Handle(ActualizarConfiguracionCommand request, CancellationToken ct)
    {
        // Parámetros de la empresa ya existentes (overrides)
        var existentes = await db.ParametrosSistema
            .Where(p => p.EmpresaId == request.EmpresaId)
            .ToListAsync(ct);

        foreach (var upd in request.Parametros)
        {
            var meta = ParametrosCatalogo.Buscar(upd.Clave);
            if (meta is null) continue;  // clave desconocida, se ignora

            // Secreto con valor vacío => no se pisa (se conserva el actual)
            if (meta.EsSecreto && string.IsNullOrWhiteSpace(upd.Valor))
                continue;

            var existente = existentes.FirstOrDefault(p => p.Clave == upd.Clave);

            if (existente is not null)
            {
                existente.Valor       = upd.Valor;
                existente.UpdatedAt   = DateTime.UtcNow;
                existente.IsEncriptado = meta.EsSecreto;
            }
            else
            {
                await db.ParametrosSistema.AddAsync(new ParametroSistema
                {
                    EmpresaId    = request.EmpresaId,
                    Clave        = meta.Clave,
                    Valor        = upd.Valor,
                    TipoDato     = meta.Tipo == "int" ? "int" : meta.Tipo == "bool" ? "bool" : "string",
                    Descripcion  = meta.Label,
                    IsEncriptado = meta.EsSecreto,
                    IsGlobal     = false
                }, ct);
            }
        }

        await db.SaveChangesAsync(ct);

        // Si cambió la hora de ejecución, reprograma los jobs en caliente
        if (request.Parametros.Any(p => p.Clave == "RECORDATORIO_HORA_EJECUCION"))
            await scheduler.ProgramarAsync(ct);

        return Result.Ok();
    }
}
