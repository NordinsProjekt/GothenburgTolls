using Entities.Tolls;
using Factories.Validator;

namespace Factories;

public static class DailyTollSummaryFactory
{
    public static DailyTollSummary Create(DateOnly forDay, decimal amount, Guid vehicleId)
    {
        DailyTollSummaryValidator.ValidateForDay(forDay);
        DailyTollSummaryValidator.ValidateAmount(amount);
        DailyTollSummaryValidator.ValidateVehicleId(vehicleId);

        return new DailyTollSummary(forDay, amount, vehicleId);
    }
}
