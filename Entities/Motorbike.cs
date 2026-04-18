using Entities.Bases;

namespace Entities;

public class Motorbike(string registrationNumber) : Vehicle(registrationNumber)
{
    private Motorbike() : this(string.Empty)
    { }
}
