using MediatR;
using Microsoft.EntityFrameworkCore;
using TurnoSys.Application.Common.Interfaces;

namespace TurnoSys.Application.Features.Pacientes.Queries.GetPacienteById;

public class GetPacienteByIdQueryHandler(IApplicationDbContext db)
    : IRequestHandler<GetPacienteByIdQuery, PacienteDetalleDto?>
{
    public async Task<PacienteDetalleDto?> Handle(GetPacienteByIdQuery request, CancellationToken ct)
    {
        var hoy = DateOnly.FromDateTime(DateTime.UtcNow);

        return await db.Pacientes
            .AsNoTracking()
            .Where(p => p.Id == request.Id && p.EmpresaId == request.EmpresaId)
            .Select(p => new PacienteDetalleDto(
                p.Id,
                p.Nombre,
                p.Apellido,
                p.Apellido + ", " + p.Nombre,
                p.DNI,
                p.FechaNacimiento,
                p.FechaNacimiento == null ? null
                    : hoy.Year - p.FechaNacimiento.Value.Year
                      - (hoy.DayOfYear < p.FechaNacimiento.Value.DayOfYear ? 1 : 0),
                p.Telefono,
                p.Email,
                p.Direccion,
                p.ObraSocial,
                p.NumeroAfiliado,
                p.ContactoEmergenciaNombre,
                p.ContactoEmergenciaTelefono,
                p.Observaciones,
                p.Restricciones,
                p.ConsentimientoFirmado,
                p.FechaConsentimiento,
                p.IsActivo,
                p.CreatedAt
            ))
            .FirstOrDefaultAsync(ct);
    }
}
