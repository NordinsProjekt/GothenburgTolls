using Entities.Interfaces;

namespace UseCases.Interfaces;

public interface ITollCalculator
{
    int CalculateDailyTotalFee(IVehicle vehicle, DateTimeOffset[] dates);
}