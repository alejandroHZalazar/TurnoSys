using MediatR;
using Microsoft.EntityFrameworkCore;
using TurnoSys.Application.Common.Interfaces;
using TurnoSys.Application.Features.Practicas.Queries.GetPracticas;

namespace TurnoSys.Application.Features.Practicas.Queries.GetPracticaById;

public class GetPracticaByIdQueryHandler(IApplicationDbContext db)
    : IRequestHandler<GetPracticaByIdQuery, PracticaListDto?>
{
    public async Task<PracticaListDto?> Handle(GetPracticaByIdQuery request, CancellationToken ct)
    {
        return await db.Practicas
            .AsNoTracking()
            .Where(p => p.Id == request.Id && p.EmpresaId == request.EmpresaId)
            .Select(p => new PracticaListDto(
                p.Id, p.Nombre, p.Descripcion, p.Precio, p.DuracionMinutos,
                p.Color, p.RequiereObservaciones, p.RecordatorioRecDias, p.IsActivo,
                p.CategoriaId,
                p.Categoria != null ? p.Categoria.Nombre : null,
                p.Categoria != null ? p.Categoria.Color : null
            ))
            .FirstOrDefaultAsync(ct);
    }
}
