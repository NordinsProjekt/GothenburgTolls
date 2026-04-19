using Entities.Bases;

namespace Entities.Vehicels;

public class Foreign(string registrationNumber) : Vehicle(registrationNumber)
{
    private Foreign() : this(string.Empty)
    { }
}
