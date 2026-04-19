using Entities.Tolls;

namespace Factories;

public static class TollEventFactory
{
    public static TollEvent Create(DateTime eventDateTime, string zone, Guid vehicleId)
    {
        TollEventValidator.ValidateEventDateTime(eventDateTime);
        var normalizedZone = TollEventValidator.ValidateAndNormalizeZone(zone);
        TollEventValidator.ValidateVehicleId(vehicleId);

        return new TollEvent(eventDateTime, normalizedZone, vehicleId);
    }
}

