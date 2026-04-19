using Entities.Bases;

namespace Entities.Vehicels;

public class Car(string registrationNumber) : Vehicle(registrationNumber)
{
    private Car() : this(string.Empty)
    { }
}