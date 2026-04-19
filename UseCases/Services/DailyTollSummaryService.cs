using Entities.Interfaces;
using Entities.Tolls;
using Factories;
using UseCases.HelperClass;
using UseCases.Interfaces;

namespace UseCases.Services;

public class DailyTollSummaryService(
    IVehicleRepository vehicleRepository,
    ITollEventRepository tollEventRepository,
    IDailyTollSummaryRepository dailyTollSummaryRepository,
    TollCalculator tollCalculator) : IDailyTollSummaryService
{
    public async Task<DailyTollSummary> CreateAsync(string registrationNumber, DateOnly forDay, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(registrationNumber);

        var vehicle = await vehicleRepository.GetVehicleByRegistrationNumberAsync(registrationNumber, cancellationToken)
            ?? throw new InvalidOperationException($"Vehicle with registration number '{registrationNumber}' not found.");

        List<TollEvent> tollEvents = await tollEventRepository.GetAllByRegistrationAsync(registrationNumber, forDay, cancellationToken);

        DateTime[] eventDateTimes = tollEvents
            .Select(e => e.EventDateTime.LocalDateTime)
            .ToArray();

        int fee = tollCalculator.CalculateDailyTotalFee(vehicle, eventDateTimes);

        DailyTollSummary summary = DailyTollSummaryFactory.Create(forDay, fee, vehicle.Id);

        await dailyTollSummaryRepository.CreateDailyTollSummaryAsync(summary, cancellationToken);

        return summary;
    }

    public async Task<List<DailyTollSummary>> GetAllByVehicleIdAsync(Guid vehicleId, CancellationToken cancellationToken)
    {
        ArgumentOutOfRangeException.ThrowIfEqual(vehicleId, Guid.Empty);
        return await dailyTollSummaryRepository.GetAllByVehicleIdAsync(vehicleId, cancellationToken);
    }
}
