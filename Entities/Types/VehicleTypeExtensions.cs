namespace Entities.Types;

public static class VehicleTypeExtensions
{
    public static bool IsTollFree(this VehicleType vehicleType)
        => vehicleType switch
        {
            VehicleType.Car => false,
            VehicleType.Motorbike => true,
            VehicleType.Tractor => true,
            VehicleType.Emergency => true,
            VehicleType.Diplomat => true,
            VehicleType.Foreign => true,
            VehicleType.Military => true,
            _ => throw new ArgumentOutOfRangeException(nameof(vehicleType), vehicleType, $"Unknown vehicle type: {(int)vehicleType}")
        };
}
