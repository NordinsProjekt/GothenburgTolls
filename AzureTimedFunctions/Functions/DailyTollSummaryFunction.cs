using Entities.Bases;
using Entities.Interfaces;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using UseCases.Interfaces;

namespace AzureTimedFunctions.Functions;

public class DailyTollSummaryFunction(
    IVehicleRepository vehicleRepository,
    IDailyTollSummaryService dailyTollSummaryService,
    ILoggerFactory loggerFactory)
{
    private readonly ILogger _logger = loggerFactory.CreateLogger<DailyTollSummaryFunction>();

    [Function(nameof(DailyTollSummaryFunction))]
    public async Task RunAsync(
        [TimerTrigger("0 0 1 * * *")] TimerInfo timerInfo,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("DailyTollSummaryFunction triggered at: {Now}", DateTime.UtcNow);

        DateOnly yesterday = DateOnly.FromDateTime(DateTime.Today.AddDays(-1));
        _logger.LogInformation("Processing daily toll summaries for date: {Date}", yesterday);

        List<Vehicle> vehicles = await vehicleRepository.GetAllVehicleAsync(cancellationToken);
        _logger.LogInformation("Found {Count} vehicles to process", vehicles.Count);

        int successCount = 0;
        int failureCount = 0;

        foreach (Vehicle vehicle in vehicles)
        {
            try
            {
                await dailyTollSummaryService.CreateAsync(
                    vehicle.RegistrationNumber,
                    yesterday,
                    cancellationToken);

                successCount++;
                _logger.LogInformation(
                    "Successfully created daily toll summary for vehicle: {RegistrationNumber}",
                    vehicle.RegistrationNumber);
            }
            catch (InvalidOperationException ex)
            {
                failureCount++;
                _logger.LogWarning(
                    ex,
                    "Failed to create daily toll summary for vehicle: {RegistrationNumber}. Reason: {Message}",
                    vehicle.RegistrationNumber,
                    ex.Message);
            }
        }

        _logger.LogInformation(
            "DailyTollSummaryFunction completed. Success: {Success}, Failures: {Failures}",
            successCount,
            failureCount);
    }
}
