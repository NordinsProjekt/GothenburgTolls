using Entities.Bases;

namespace Entities;

public class Car(string registrationNumber) : Vehicle(registrationNumber)
{
    private Car() : this(string.Empty)
    { }
}