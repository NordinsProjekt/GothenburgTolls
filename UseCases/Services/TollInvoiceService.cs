using Entities.Interfaces;
using Entities.Tolls;
using Factories;
using UseCases.Interfaces;

namespace UseCases.Services;

public class TollInvoiceService(
    IVehicleRepository vehicleRepository,
    IDailyTollSummaryRepository dailyTollSummaryRepository,
    ITollInvoiceRepository tollInvoiceRepository) : ITollInvoiceService
{
    public async Task<TollInvoice> CreateAsync(string registrationNumber, int year, int month, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(registrationNumber);

        var vehicle = await vehicleRepository.GetVehicleByRegistrationNumberAsync(registrationNumber, cancellationToken)
            ?? throw new InvalidOperationException($"Vehicle with registration number '{registrationNumber}' not found.");

        if (await tollInvoiceRepository.ExistsAsync(vehicle.Id, year, month, cancellationToken))
        {
            throw new InvalidOperationException($"A toll invoice already exists for '{registrationNumber}' for {year}-{month:D2}.");
        }

        List<DailyTollSummary> summaries = await dailyTollSummaryRepository
            .GetUninvoicedByVehicleAndMonthAsync(vehicle.Id, year, month, cancellationToken);

        if (summaries.Count == 0)
        {
            throw new InvalidOperationException($"No uninvoiced daily toll summaries found for '{registrationNumber}' for {year}-{month:D2}.");
        }

        TollInvoice invoice = TollInvoiceFactory.Create(vehicle.Id, year, month, summaries);

        foreach (DailyTollSummary summary in summaries)
        {
            summary.AssignToTollInvoice(invoice.Id);
        }

        await tollInvoiceRepository.CreateTollInvoiceAsync(invoice, summaries, cancellationToken);

        return invoice;
    }

    public async Task<List<TollInvoice>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await tollInvoiceRepository.GetAllAsync(cancellationToken);
    }
}
