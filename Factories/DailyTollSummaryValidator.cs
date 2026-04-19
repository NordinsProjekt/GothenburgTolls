namespace Factories;

internal static class DailyTollSummaryValidator
{
    internal static void ValidateForDay(DateOnly forDay)
    {
        if (forDay == default)
        {
            throw new ArgumentException("For day is required.", nameof(forDay));
        }
    }

    internal static void ValidateAmount(decimal amount)
    {
        if (amount < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(amount),
                amount,
                "Amount cannot be negative.");
        }
    }

    internal static void ValidateVehicleId(Guid vehicleId)
    {
        if (vehicleId == Guid.Empty)
        {
            throw new ArgumentException("Vehicle id is required.", nameof(vehicleId));
        }
    }
}
