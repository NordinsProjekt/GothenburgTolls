using Entities.Bases;

namespace Entities.Vehicels;

public class Emergency(string registrationNumber) : Vehicle(registrationNumber)
{
    private Emergency() : this(string.Empty)
    { }
}
