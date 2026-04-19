namespace Factories;

internal static class TollEventValidator
{
    internal const int ZoneMaxLength = 64;

    internal static void ValidateEventDateTime(DateTimeOffset eventDateTime)
    {
        if (eventDateTime == default)
        {
            throw new ArgumentException("Event date time is required.", nameof(eventDateTime));
        }

        if (eventDateTime > DateTimeOffset.UtcNow)
        {
            throw new ArgumentOutOfRangeException(
                nameof(eventDateTime),
                eventDateTime,
                "Event date time cannot be in the future.");
        }
    }

    internal static string ValidateAndNormalizeZone(string zone)
    {
        if (string.IsNullOrWhiteSpace(zone))
        {
            throw new ArgumentException("Zone is required.", nameof(zone));
        }

        var trimmedZone = zone.Trim();
        if (trimmedZone.Length > ZoneMaxLength)
        {
            throw new ArgumentOutOfRangeException(
                nameof(zone),
                trimmedZone.Length,
                $"Zone cannot exceed {ZoneMaxLength} characters.");
        }

        return trimmedZone;
    }

    internal static void ValidateVehicleId(Guid vehicleId)
    {
        if (vehicleId == Guid.Empty)
        {
            throw new ArgumentException("Vehicle id is required.", nameof(vehicleId));
        }
    }
}
