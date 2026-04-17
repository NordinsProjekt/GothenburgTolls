using Entities.Interfaces;

namespace Entities;

public class Car : IVehicle
{
    public string GetVehicleType() => nameof(Car);
}