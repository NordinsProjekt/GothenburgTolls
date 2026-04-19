using Entities.Tolls;

namespace Factories;

public static class TollEventFactory
{
    public static TollEvent Create(DateTime eventDateTime, string zone, Guid vehicleId)
    {
        if (eventDateTime == default)
        {
            throw new ArgumentException("Event date time is required.", nameof(eventDateTime));
        }

        if (eventDateTime > DateTime.UtcNow.AddMinutes(5))
        {
            throw new ArgumentOutOfRangeException(
                nameof(eventDateTime),
                eventDateTime,
                "Event date time cannot be in the future.");
        }

        if (string.IsNullOrWhiteSpace(zone))
        {
            throw new ArgumentException("Zone is required.", nameof(zone));
        }

        if (vehicleId == Guid.Empty)
        {
            throw new ArgumentException("Vehicle id is required.", nameof(vehicleId));
        }

        return new TollEvent(eventDateTime, zone.Trim(), vehicleId);
    }
}
