using Entities.Types;

namespace Factories.Validator;

internal static class VehicleValidator
{
    internal static string ValidateAndNormalizeRegistrationNumber(string registrationNumber)
    {
        if (string.IsNullOrWhiteSpace(registrationNumber))
        {
            throw new ArgumentException("Registration number is required.", nameof(registrationNumber));
        }

        return registrationNumber.Trim();
    }

    internal static void ValidateVehicleType(VehicleType vehicleType)
    {
        if (!Enum.IsDefined(vehicleType))
        {
            throw new ArgumentOutOfRangeException(
                nameof(vehicleType),
                vehicleType,
                $"Unsupported vehicle type: {(int)vehicleType} is not a defined value of {nameof(VehicleType)}.");
        }
    }
}
