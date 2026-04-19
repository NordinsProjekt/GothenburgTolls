namespace Entities.Types;

public static class VehicleTypeExtensions
{
    /// <summary>
    /// Returnerar true om fordonstypen är undantagen från trängselskatt.
    /// Endast <see cref="VehicleType.Car"/> är avgiftsbelagd.
    /// </summary>
    public static bool IsTollFree(this VehicleType vehicleType)
        => vehicleType != VehicleType.Car;
}
