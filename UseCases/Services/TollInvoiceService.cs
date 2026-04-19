using Entities.Bases;
using Entities.Interfaces;
using Entities.Tolls;
using Factories;
using UseCases.Interfaces;
using UseCases.Validators;

namespace UseCases.Services;

public class TollInvoiceService(
    IVehicleRepository vehicleRepository,
    IDailyTollSummaryRepository dailyTollSummaryRepository,
    ITollInvoiceRepository tollInvoiceRepository) : ITollInvoiceService
{
    public async Task<TollInvoice> CreateAsync(string registrationNumber, int year, int month, CancellationToken cancellationToken)
    {
        registrationNumber = TollInvoiceServiceValidator.ValidateAndNormalizeRegistrationNumber(registrationNumber);
        TollInvoiceServiceValidator.ValidateYear(year);
        TollInvoiceServiceValidator.ValidateMonth(month);

        Vehicle vehicle = await GetRequiredVehicleAsync(registrationNumber, cancellationToken);
        await EnsureNoDuplicateInvoiceAsync(vehicle.Id, registrationNumber, year, month, cancellationToken);
        List<DailyTollSummary> summaries = await GetRequiredSummariesAsync(vehicle.Id, registrationNumber, year, month, cancellationToken);

        TollInvoice invoice = TollInvoiceFactory.Create(vehicle.Id, year, month, summaries);

        AssignSummariesToInvoice(summaries, invoice.Id);
        await tollInvoiceRepository.CreateTollInvoiceAsync(invoice, summaries, cancellationToken);

        return invoice;
    }

    public async Task<List<TollInvoice>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await tollInvoiceRepository.GetAllAsync(cancellationToken);
    }

    private async Task<Vehicle> GetRequiredVehicleAsync(string registrationNumber, CancellationToken cancellationToken)
    {
        return await vehicleRepository.GetVehicleByRegistrationNumberAsync(registrationNumber, cancellationToken)
            ?? throw new InvalidOperationException($"Vehicle with registration number '{registrationNumber}' not found.");
    }

    private async Task EnsureNoDuplicateInvoiceAsync(Guid vehicleId, string registrationNumber, int year, int month, CancellationToken cancellationToken)
    {
        if (await tollInvoiceRepository.ExistsAsync(vehicleId, year, month, cancellationToken))
        {
            throw new InvalidOperationException($"A toll invoice already exists for '{registrationNumber}' for {year}-{month:D2}.");
        }
    }

    private async Task<List<DailyTollSummary>> GetRequiredSummariesAsync(Guid vehicleId, string registrationNumber, int year, int month, CancellationToken cancellationToken)
    {
        List<DailyTollSummary> summaries = await dailyTollSummaryRepository
            .GetUninvoicedByVehicleAndMonthAsync(vehicleId, year, month, cancellationToken);

        if (summaries.Count == 0)
        {
            throw new InvalidOperationException($"No uninvoiced daily toll summaries found for '{registrationNumber}' for {year}-{month:D2}.");
        }

        return summaries;
    }

    private static void AssignSummariesToInvoice(List<DailyTollSummary> summaries, Guid invoiceId)
    {
        foreach (DailyTollSummary summary in summaries)
        {
            summary.AssignToTollInvoice(invoiceId);
        }
    }
}
