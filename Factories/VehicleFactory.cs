using Entities.Bases;
using Entities.Types;
using Entities.Vehicels;

namespace Factories;

public static class VehicleFactory
{
    public static Vehicle Create(string registrationNumber, VehicleType vehicleType)
    {
        if (string.IsNullOrWhiteSpace(registrationNumber))
        {
            throw new ArgumentException("Registration number is required.", nameof(registrationNumber));
        }

        if (!Enum.IsDefined(vehicleType))
        {
            throw new ArgumentOutOfRangeException(
                nameof(vehicleType),
                vehicleType,
                $"Unsupported vehicle type: {(int)vehicleType} is not a defined value of {nameof(VehicleType)}.");
        }

        return vehicleType switch
        {
            VehicleType.Car => new Car(registrationNumber),
            VehicleType.Motorbike => new Motorbike(registrationNumber),
            VehicleType.Tractor => new Tractor(registrationNumber),
            VehicleType.Emergency => new Emergency(registrationNumber),
            VehicleType.Diplomat => new Diplomat(registrationNumber),
            VehicleType.Foreign => new Foreign(registrationNumber),
            VehicleType.Military => new Military(registrationNumber),
            _ => throw new ArgumentOutOfRangeException(
                nameof(vehicleType),
                vehicleType,
                $"Vehicle type {vehicleType} ({(int)vehicleType}) is defined but has no factory mapping."),
        };
    }
}
