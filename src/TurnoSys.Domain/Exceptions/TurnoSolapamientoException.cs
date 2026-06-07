namespace TurnoSys.Domain.Exceptions;

public class TurnoSolapamientoException : DomainException
{
    public TurnoSolapamientoException()
        : base("El profesional ya tiene un turno en ese horario.") { }

    public TurnoSolapamientoException(string profesional, DateTime inicio, DateTime fin)
        : base($"El profesional '{profesional}' ya tiene un turno entre {inicio:HH:mm} y {fin:HH:mm}.") { }
}
