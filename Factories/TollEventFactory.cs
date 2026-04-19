using Entities.Tolls;
using Factories.Validator;

namespace Factories;

public static class TollEventFactory
{
    public static TollEvent Create(DateTimeOffset eventDateTime, string zone, Guid vehicleId)
    {
        TollEventValidator.ValidateEventDateTime(eventDateTime);
        var normalizedZone = TollEventValidator.ValidateAndNormalizeZone(zone);
        TollEventValidator.ValidateVehicleId(vehicleId);

        return new TollEvent(eventDateTime, normalizedZone, vehicleId);
    }
}

