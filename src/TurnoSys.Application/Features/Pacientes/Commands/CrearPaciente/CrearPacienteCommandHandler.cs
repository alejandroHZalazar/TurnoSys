using MediatR;
using Microsoft.EntityFrameworkCore;
using TurnoSys.Application.Common.Interfaces;
using TurnoSys.Application.Common.Models;
using TurnoSys.Domain.Entities;

namespace TurnoSys.Application.Features.Pacientes.Commands.CrearPaciente;

public class CrearPacienteCommandHandler(IApplicationDbContext db)
    : IRequestHandler<CrearPacienteCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CrearPacienteCommand request, CancellationToken ct)
    {
        // Verificar DNI duplicado en la misma empresa
        if (!string.IsNullOrWhiteSpace(request.DNI))
        {
            var existe = await db.Pacientes.AnyAsync(p =>
                p.EmpresaId == request.EmpresaId &&
                p.DNI == request.DNI.Trim() &&
                !p.IsDeleted, ct);

            if (existe)
                return Result.Fail<Guid>($"Ya existe un paciente con DNI {request.DNI} en esta empresa.");
        }

        var paciente = new Paciente
        {
            EmpresaId                 = request.EmpresaId,
            Nombre                    = request.Nombre.Trim(),
            Apellido                  = request.Apellido.Trim(),
            DNI                       = request.DNI?.Trim(),
            FechaNacimiento           = request.FechaNacimiento,
            Telefono                  = request.Telefono?.Trim(),
            Email                     = request.Email?.Trim().ToLower(),
            Direccion                 = request.Direccion?.Trim(),
            ObraSocial                = request.ObraSocial?.Trim(),
            NumeroAfiliado            = request.NumeroAfiliado?.Trim(),
            ContactoEmergenciaNombre  = request.ContactoEmergenciaNombre?.Trim(),
            ContactoEmergenciaTelefono = request.ContactoEmergenciaTelefono?.Trim(),
            Observaciones             = request.Observaciones?.Trim(),
            Restricciones             = request.Restricciones?.Trim(),
            IsActivo                  = true
        };

        await db.Pacientes.AddAsync(paciente, ct);
        await db.SaveChangesAsync(ct);

        return Result.Ok(paciente.Id);
    }
}
