using Entities.Bases;
using Entities.Interfaces;
using Entities.Tolls;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using UseCases.Interfaces;

namespace AzureTimedFunctions.Functions;

public class MonthlyTollInvoiceFunction(
    IVehicleRepository vehicleRepository,
    ITollInvoiceService tollInvoiceService,
    ILoggerFactory loggerFactory)
{
    private readonly ILogger _logger = loggerFactory.CreateLogger<MonthlyTollInvoiceFunction>();

    [Function(nameof(MonthlyTollInvoiceFunction))]
    public async Task RunAsync(
        [TimerTrigger("0 0 2 1 * *")] TimerInfo timerInfo,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("MonthlyTollInvoiceFunction triggered at: {Now}", DateTime.UtcNow);

        DateOnly today = SwedishTimeHelper.Today();
        DateOnly previousMonth = today.AddMonths(-1);
        int year = previousMonth.Year;
        int month = previousMonth.Month;

        _logger.LogInformation("Processing monthly toll invoices for: {Year}-{Month:D2}", year, month);

        List<Vehicle> vehicles = await vehicleRepository.GetAllVehicleAsync(cancellationToken);
        _logger.LogInformation("Found {Count} vehicles to process", vehicles.Count);

        int successCount = 0;
        int failureCount = 0;

        foreach (Vehicle vehicle in vehicles)
        {
            try
            {
                await tollInvoiceService.CreateAsync(
                    vehicle.RegistrationNumber,
                    year,
                    month,
                    cancellationToken);

                successCount++;
                _logger.LogInformation(
                    "Successfully created toll invoice for vehicle: {RegistrationNumber}",
                    vehicle.RegistrationNumber);
            }
            catch (InvalidOperationException ex)
            {
                failureCount++;
                _logger.LogWarning(
                    ex,
                    "Failed to create toll invoice for vehicle: {RegistrationNumber}. Reason: {Message}",
                    vehicle.RegistrationNumber,
                    ex.Message);
            }
        }

        _logger.LogInformation(
            "MonthlyTollInvoiceFunction completed. Success: {Success}, Failures: {Failures}",
            successCount,
            failureCount);
    }
}
