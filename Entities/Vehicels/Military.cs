using Entities.Bases;

namespace Entities.Vehicels;

public class Military(string registrationNumber) : Vehicle(registrationNumber)
{
    private Military() : this(string.Empty)
    { }
}
