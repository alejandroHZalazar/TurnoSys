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
        // FechaHoraInicio viene en "wall-clock" (hora local del negocio),
        // así que comparamos contra DateTime.Now (también local), no UtcNow.
        RuleFor(x => x.FechaHoraInicio)
            .NotEmpty()
            .GreaterThan(DateTime.Now.AddMinutes(-5))
            .WithMessage("No se puede reservar un turno en el pasado.");
        RuleFor(x => x.Observaciones).MaximumLength(2000).When(x => x.Observaciones != null);
    }
}
