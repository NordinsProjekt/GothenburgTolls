using Entities.Bases;

namespace Entities.Vehicels;

public class Motorbike(string registrationNumber) : Vehicle(registrationNumber)
{
    private Motorbike() : this(string.Empty)
    { }
}
