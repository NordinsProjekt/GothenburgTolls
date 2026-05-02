using Entities.Bases;
using Entities.Interfaces;
using Entities.Tolls;
using Factories;
using UseCases.Exceptions;
using UseCases.Interfaces;
using UseCases.Results;
using UseCases.Validators;

namespace UseCases.Services;

public class DailyTollSummaryService(
    IVehicleRepository vehicleRepository,
    ITollEventRepository tollEventRepository,
    IDailyTollSummaryRepository dailyTollSummaryRepository,
    ITollCalculator tollCalculator) : IDailyTollSummaryService
{
    public async Task<DailyTollSummary> CreateAsync(string registrationNumber, DateOnly forDay, CancellationToken cancellationToken)
    {
        registrationNumber = DailyTollSummaryServiceValidator.ValidateAndNormalizeRegistrationNumber(registrationNumber);
        DailyTollSummaryServiceValidator.ValidateForDay(forDay);

        Vehicle vehicle = await GetRequiredVehicleAsync(registrationNumber, cancellationToken);
        await EnsureNoDuplicateSummaryAsync(vehicle.Id, registrationNumber, forDay, cancellationToken);
        List<TollEvent> tollEvents = await GetRequiredTollEventsAsync(registrationNumber, forDay, cancellationToken);

        int fee = CalculateFee(vehicle, tollEvents);
        DailyTollSummary summary = DailyTollSummaryFactory.Create(forDay, fee, vehicle.Id);

        AssignTollEventsToSummary(tollEvents, summary.Id);
        await dailyTollSummaryRepository.CreateWithTollEventsAsync(summary, tollEvents, cancellationToken);

        return summary;
    }

    private async Task<Vehicle> GetRequiredVehicleAsync(string registrationNumber, CancellationToken cancellationToken)
    {
        return await vehicleRepository.GetVehicleByRegistrationNumberAsync(registrationNumber, cancellationToken)
            ?? throw new InvalidOperationException($"Vehicle with registration number '{registrationNumber}' not found.");
    }

    private async Task EnsureNoDuplicateSummaryAsync(Guid vehicleId, string registrationNumber, DateOnly forDay, CancellationToken cancellationToken)
    {
        if (await dailyTollSummaryRepository.ExistsAsync(vehicleId, forDay, cancellationToken))
        {
            throw new InvalidOperationException($"A daily toll summary already exists for '{registrationNumber}' on {forDay}.");
        }
    }

    private async Task<List<TollEvent>> GetRequiredTollEventsAsync(string registrationNumber, DateOnly forDay, CancellationToken cancellationToken)
    {
        List<TollEvent> tollEvents = await tollEventRepository.GetAllByRegistrationAsync(registrationNumber, forDay, cancellationToken);

        if (tollEvents.Count == 0)
        {
            throw new InvalidOperationException($"No unprocessed toll events found for '{registrationNumber}' on {forDay}.");
        }

        return tollEvents;
    }

    private int CalculateFee(Vehicle vehicle, List<TollEvent> tollEvents)
    {
        DateTimeOffset[] eventDateTimes = tollEvents
            .Select(e => e.EventDateTime)
            .ToArray();

        return tollCalculator.CalculateDailyTotalFee(vehicle, eventDateTimes);
    }

    private static void AssignTollEventsToSummary(List<TollEvent> tollEvents, Guid summaryId)
    {
        foreach (TollEvent tollEvent in tollEvents)
        {
            tollEvent.AssignToDailyTollSummary(summaryId);
        }
    }

    public async Task<BackfillResult> BackfillMissedAsync(CancellationToken cancellationToken)
    {
        DateOnly today = SwedishTimeHelper.Today();
        List<TollEvent> unassigned = await tollEventRepository.GetUnassignedBeforeDateAsync(today, cancellationToken);

        IEnumerable<IGrouping<(Guid VehicleId, DateOnly Day), TollEvent>> groups = unassigned
            .Where(te => te.VehicleId.HasValue)
            .GroupBy(te => (te.VehicleId!.Value, SwedishTimeHelper.ToDate(te.EventDateTime)));

        int created = 0;
        int skipped = 0;
        int failed = 0;

        foreach (IGrouping<(Guid VehicleId, DateOnly Day), TollEvent> group in groups)
        {
            try
            {
                Vehicle vehicle = await RunOrThrowSkippedAsync(
                    () => vehicleRepository.GetVehicleByIdAsync(group.Key.VehicleId, cancellationToken));
                await RunOrThrowSkippedAsync(
                    () => CreateAsync(vehicle.RegistrationNumber, group.Key.Day, cancellationToken));
                created++;
            }
            catch (BackfillSkippedException)
            {
                skipped++;
            }
            catch
            {
                failed++;
            }
        }

        return new BackfillResult(created, skipped, failed);
    }

    private static async Task<T> RunOrThrowSkippedAsync<T>(Func<Task<T>> action)
    {
        try
        {
            return await action();
        }
        catch (InvalidOperationException ex)
        {
            throw new BackfillSkippedException(ex.Message, ex);
        }
    }

    private static async Task RunOrThrowSkippedAsync(Func<Task> action) =>
        await RunOrThrowSkippedAsync<bool>(async () => { await action(); return true; });

    public async Task<List<DailyTollSummary>> GetAllByVehicleIdAsync(Guid vehicleId, CancellationToken cancellationToken)
    {
        DailyTollSummaryServiceValidator.ValidateVehicleId(vehicleId);
        return await dailyTollSummaryRepository.GetAllByVehicleIdAsync(vehicleId, cancellationToken);
    }

    public async Task<List<DailyTollSummary>> GetAllUninvoicedAsync(CancellationToken cancellationToken)
    {
        return await dailyTollSummaryRepository.GetAllUninvoicedAsync(cancellationToken);
    }

    public async Task<List<DailyTollSummary>> GetUninvoicedByVehicleIdAsync(Guid vehicleId, CancellationToken cancellationToken)
    {
        DailyTollSummaryServiceValidator.ValidateVehicleId(vehicleId);
        return await dailyTollSummaryRepository.GetUninvoicedByVehicleIdAsync(vehicleId, cancellationToken);
    }
}
