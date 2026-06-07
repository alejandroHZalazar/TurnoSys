using MediatR;
using Microsoft.EntityFrameworkCore;
using TurnoSys.Application.Common.Exceptions;
using TurnoSys.Application.Common.Interfaces;
using TurnoSys.Application.Common.Models;
using TurnoSys.Domain.Entities;

namespace TurnoSys.Application.Features.Pacientes.Commands.ActualizarPaciente;

public class ActualizarPacienteCommandHandler(IApplicationDbContext db)
    : IRequestHandler<ActualizarPacienteCommand, Result>
{
    public async Task<Result> Handle(ActualizarPacienteCommand request, CancellationToken ct)
    {
        var paciente = await db.Pacientes
            .FirstOrDefaultAsync(p => p.Id == request.Id && p.EmpresaId == request.EmpresaId, ct)
            ?? throw new NotFoundException(nameof(Paciente), request.Id);

        // Verificar DNI duplicado (excluyendo el paciente actual)
        if (!string.IsNullOrWhiteSpace(request.DNI))
        {
            var existe = await db.Pacientes.AnyAsync(p =>
                p.EmpresaId == request.EmpresaId &&
                p.DNI == request.DNI.Trim() &&
                p.Id != request.Id &&
                !p.IsDeleted, ct);

            if (existe)
                return Result.Fail($"Ya existe otro paciente con DNI {request.DNI}.");
        }

        paciente.Nombre                     = request.Nombre.Trim();
        paciente.Apellido                   = request.Apellido.Trim();
        paciente.DNI                        = request.DNI?.Trim();
        paciente.FechaNacimiento            = request.FechaNacimiento;
        paciente.Telefono                   = request.Telefono?.Trim();
        paciente.Email                      = request.Email?.Trim().ToLower();
        paciente.Direccion                  = request.Direccion?.Trim();
        paciente.ObraSocial                 = request.ObraSocial?.Trim();
        paciente.NumeroAfiliado             = request.NumeroAfiliado?.Trim();
        paciente.ContactoEmergenciaNombre   = request.ContactoEmergenciaNombre?.Trim();
        paciente.ContactoEmergenciaTelefono = request.ContactoEmergenciaTelefono?.Trim();
        paciente.Observaciones              = request.Observaciones?.Trim();
        paciente.Restricciones              = request.Restricciones?.Trim();
        paciente.IsActivo                   = request.IsActivo;
        paciente.UpdatedAt                  = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);
        return Result.Ok();
    }
}
