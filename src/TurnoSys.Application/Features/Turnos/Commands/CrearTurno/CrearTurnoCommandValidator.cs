using FluentValidation;

namespace TurnoSys.Application.Features.Turnos.Commands.CrearTurno;

public class CrearTurnoCommandValidator : AbstractValidator<CrearTurnoCommand>
{
    public CrearTurnoCommandValidator()
    {
        RuleFor(x => x.EmpresaId).NotEmpty();
        RuleFor(x => x.ProfesionalId).NotEmpty();
        RuleFor(x => x.PacienteId).NotEmpty();
        RuleFor(x => x.PracticaId).NotEmpty();
        // FechaHoraInicio viene en "wall-clock" de Argentina (UTC-3 fijo, sin DST).
        // "ahora AR" se calcula desde UtcNow en cada validación (Must), para no
        // depender de la zona horaria del servidor (Railway corre en UTC).
        RuleFor(x => x.FechaHoraInicio)
            .NotEmpty()
            .Must(fecha => fecha > DateTime.UtcNow.AddHours(-3).AddMinutes(-5))
            .WithMessage("No se puede reservar un turno en el pasado.");
        RuleFor(x => x.Observaciones).MaximumLength(2000).When(x => x.Observaciones != null);
    }
}
