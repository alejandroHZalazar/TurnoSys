using MediatR;
using Microsoft.EntityFrameworkCore;
using TurnoSys.Application.Common.Interfaces;

namespace TurnoSys.Application.Features.Practicas.Queries.GetPracticas;

public class GetPracticasQueryHandler(IApplicationDbContext db)
    : IRequestHandler<GetPracticasQuery, IEnumerable<PracticaListDto>>
{
    public async Task<IEnumerable<PracticaListDto>> Handle(GetPracticasQuery request, CancellationToken ct)
    {
        var query = db.Practicas
            .AsNoTracking()
            .Where(p => p.EmpresaId == request.EmpresaId);

        if (request.SoloActivas == true)
            query = query.Where(p => p.IsActivo);

        return await query
            .OrderBy(p => p.Categoria != null ? p.Categoria.Orden : 999)
            .ThenBy(p => p.Nombre)
            .Select(p => new PracticaListDto(
                p.Id,
                p.Nombre,
                p.Descripcion,
                p.Precio,
                p.DuracionMinutos,
                p.Color,
                p.RequiereObservaciones,
                p.RecordatorioRecDias,
                p.IsActivo,
                p.CategoriaId,
                p.Categoria != null ? p.Categoria.Nombre : null,
                p.Categoria != null ? p.Categoria.Color : null
            ))
            .ToListAsync(ct);
    }
}
