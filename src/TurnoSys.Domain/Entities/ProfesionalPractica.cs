namespace TurnoSys.Domain.Entities;

public class ProfesionalPractica
{
    public Guid ProfesionalId { get; set; }
    public Guid PracticaId { get; set; }

    public Profesional Profesional { get; set; } = null!;
    public Practica Practica { get; set; } = null!;
}
