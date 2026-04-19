namespace UseCases.Validators;

internal static class DailyTollSummaryServiceValidator
{
    internal static string ValidateAndNormalizeRegistrationNumber(string registrationNumber)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(registrationNumber);
        return registrationNumber.Trim();
    }

    internal static void ValidateForDay(DateOnly forDay)
    {
        if (forDay >= DateOnly.FromDateTime(DateTime.Today))
        {
            throw new ArgumentOutOfRangeException(nameof(forDay), forDay, "Cannot create a daily toll summary for today or a future date.");
        }
    }

    internal static void ValidateVehicleId(Guid vehicleId)
    {
        ArgumentOutOfRangeException.ThrowIfEqual(vehicleId, Guid.Empty);
    }
}
