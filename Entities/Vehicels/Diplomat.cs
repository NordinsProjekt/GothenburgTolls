using Entities.Bases;

namespace Entities.Vehicels;

public class Diplomat(string registrationNumber) : Vehicle(registrationNumber)
{
    private Diplomat() : this(string.Empty)
    { }
}
