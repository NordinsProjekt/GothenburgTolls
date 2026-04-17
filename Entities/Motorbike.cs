using Entities.Interfaces;

namespace Entities;

public class Motorbike : IVehicle
{
    public string GetVehicleType() => nameof(Motorbike);
}
