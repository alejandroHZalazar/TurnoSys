using MediatR;
using Microsoft.EntityFrameworkCore;
using TurnoSys.Application.Common.Interfaces;
using TurnoSys.Application.Common.Models;

namespace TurnoSys.Application.Features.Pacientes.Queries.GetPacientes;

public class GetPacientesQueryHandler(IApplicationDbContext db)
    : IRequestHandler<GetPacientesQuery, PagedResult<PacienteListDto>>
{
    public async Task<PagedResult<PacienteListDto>> Handle(GetPacientesQuery request, CancellationToken ct)
    {
        var query = db.Pacientes
            .AsNoTracking()
            .Where(p => p.EmpresaId == request.EmpresaId);

        if (request.SoloActivos == true)
            query = query.Where(p => p.IsActivo);

        if (!string.IsNullOrWhiteSpace(request.Busqueda))
        {
            var b = request.Busqueda.Trim().ToLower();
            query = query.Where(p =>
                p.Nombre.ToLower().Contains(b) ||
                p.Apellido.ToLower().Contains(b) ||
                (p.DNI != null && p.DNI.Contains(b)) ||
                (p.Telefono != null && p.Telefono.Contains(b)));
        }

        var total = await query.CountAsync(ct);

        var hoy = DateOnly.FromDateTime(DateTime.UtcNow);

        var items = await query
            .OrderBy(p => p.Apellido)
            .ThenBy(p => p.Nombre)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(p => new PacienteListDto(
                p.Id,
                p.Nombre,
                p.Apellido,
                p.Apellido + ", " + p.Nombre,
                p.DNI,
                p.Telefono,
                p.Email,
                p.ObraSocial,
                p.FechaNacimiento == null ? null
                    : hoy.Year - p.FechaNacimiento.Value.Year
                      - (hoy.DayOfYear < p.FechaNacimiento.Value.DayOfYear ? 1 : 0),
                p.IsActivo,
                db.Turnos.Count(t => t.PacienteId == p.Id && !t.IsDeleted)
            ))
            .ToListAsync(ct);

        return new PagedResult<PacienteListDto>
        {
            Items = items,
            Page = request.Page,
            PageSize = request.PageSize,
            TotalItems = total
        };
    }
}
