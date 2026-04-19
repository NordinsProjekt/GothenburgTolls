using Entities.Bases;

namespace Entities.Vehicles;

public class Tractor(string registrationNumber) : Vehicle(registrationNumber)
{
    private Tractor() : this(string.Empty)
    { }
}
