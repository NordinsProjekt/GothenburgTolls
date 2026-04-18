using Entities.Interfaces;

namespace Entities.Bases;

public abstract class Vehicle : IVehicle
{
    public Guid Id { get; }

    public string RegistrationNumber { get; }
    protected List<TollEvent> TollEvents { get; } = new();

    public Vehicle(string registrationNumber)
    {
        RegistrationNumber = registrationNumber;
    }

    public string GetVehicleType()
    {
        return GetType().Name;
    }
}
