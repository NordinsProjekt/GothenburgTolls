using Entities.Interfaces;

namespace Entities.Bases;

public abstract class Vehicle : IVehicle
{
    public Guid Id { get; }

    public string RegistrationNumber { get; }

    public string GetVehicleType()
    {
        return nameof(Vehicle);
    }
}
