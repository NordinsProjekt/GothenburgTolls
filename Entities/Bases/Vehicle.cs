using Entities.Interfaces;

namespace Entities.Bases;

public abstract class Vehicle(string registrationNumber) : IVehicle
{
    public Guid Id { get; set; }

    public string RegistrationNumber { get; } = registrationNumber;

    public string GetVehicleType()
    {
        return GetType().Name;
    }
}
