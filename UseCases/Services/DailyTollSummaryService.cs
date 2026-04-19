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

        if (forDay >= DateOnly.FromDateTime(DateTime.Today))
        {
            throw new ArgumentOutOfRangeException(nameof(forDay), forDay, "Cannot create a daily toll summary for today or a future date.");
        }

        var vehicle = await vehicleRepository.GetVehicleByRegistrationNumberAsync(registrationNumber, cancellationToken)
            ?? throw new InvalidOperationException($"Vehicle with registration number '{registrationNumber}' not found.");

        if (await dailyTollSummaryRepository.ExistsAsync(vehicle.Id, forDay, cancellationToken))
        {
            throw new InvalidOperationException($"A daily toll summary already exists for '{registrationNumber}' on {forDay}.");
        }

        List<TollEvent> tollEvents = await tollEventRepository.GetAllByRegistrationAsync(registrationNumber, forDay, cancellationToken);

        if (tollEvents.Count == 0)
        {
            throw new InvalidOperationException($"No unprocessed toll events found for '{registrationNumber}' on {forDay}.");
        }

        DateTime[] eventDateTimes = tollEvents
            .Select(e => e.EventDateTime.LocalDateTime)
            .ToArray();

        int fee = tollCalculator.CalculateDailyTotalFee(vehicle, eventDateTimes);

        DailyTollSummary summary = DailyTollSummaryFactory.Create(forDay, fee, vehicle.Id);

        await dailyTollSummaryRepository.CreateDailyTollSummaryAsync(summary, cancellationToken);

        foreach (TollEvent tollEvent in tollEvents)
        {
            tollEvent.AssignToDailyTollSummary(summary.Id);
        }

        await tollEventRepository.UpdateTollEventsAsync(tollEvents, cancellationToken);

        return summary;
    }

    public async Task<List<DailyTollSummary>> GetAllByVehicleIdAsync(Guid vehicleId, CancellationToken cancellationToken)
    {
        ArgumentOutOfRangeException.ThrowIfEqual(vehicleId, Guid.Empty);
        return await dailyTollSummaryRepository.GetAllByVehicleIdAsync(vehicleId, cancellationToken);
    }
}
