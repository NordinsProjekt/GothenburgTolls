using Entities.Bases;

namespace Entities.Vehicels;

public class Tractor(string registrationNumber) : Vehicle(registrationNumber)
{
    private Tractor() : this(string.Empty)
    { }
}
