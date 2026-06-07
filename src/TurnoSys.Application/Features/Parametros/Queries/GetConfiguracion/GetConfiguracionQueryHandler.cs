using MediatR;
using Microsoft.EntityFrameworkCore;
using TurnoSys.Application.Common.Interfaces;

namespace TurnoSys.Application.Features.Parametros.Queries.GetConfiguracion;

public class GetConfiguracionQueryHandler(IApplicationDbContext db)
    : IRequestHandler<GetConfiguracionQuery, IEnumerable<SeccionConfigDto>>
{
    public async Task<IEnumerable<SeccionConfigDto>> Handle(GetConfiguracionQuery request, CancellationToken ct)
    {
        var claves = ParametrosCatalogo.Todos.Select(p => p.Clave).ToList();

        // Trae parámetros de la empresa + globales para esas claves
        var existentes = await db.ParametrosSistema
            .AsNoTracking()
            .Where(p => claves.Contains(p.Clave) &&
                        (p.EmpresaId == request.EmpresaId || p.EmpresaId == null))
            .Select(p => new { p.Clave, p.Valor, p.EmpresaId })
            .ToListAsync(ct);

        // Valor efectivo: prioriza el de empresa sobre el global
        string ValorEfectivo(string clave, string def)
        {
            var deEmpresa = existentes.FirstOrDefault(p => p.Clave == clave && p.EmpresaId == request.EmpresaId);
            if (deEmpresa is not null) return deEmpresa.Valor ?? def;
            var global = existentes.FirstOrDefault(p => p.Clave == clave && p.EmpresaId == null);
            return global?.Valor ?? def;
        }

        return ParametrosCatalogo.Todos
            .GroupBy(p => p.Seccion)
            .Select(g => new SeccionConfigDto(
                g.Key,
                g.Select(meta =>
                {
                    var valor = ValorEfectivo(meta.Clave, meta.DefaultValor);
                    return new ParametroConfigDto(
                        meta.Clave,
                        meta.Label,
                        meta.Tipo,
                        meta.EsSecreto ? "" : valor,
                        meta.EsSecreto,
                        meta.EsSecreto && !string.IsNullOrEmpty(valor),
                        meta.Opciones,
                        meta.Ayuda
                    );
                }).ToList()
            ))
            .ToList();
    }
}
