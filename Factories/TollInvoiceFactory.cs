using Entities.Tolls;

namespace Factories;

public static class TollInvoiceFactory
{
    public static TollInvoice Create(Guid vehicleId, int year, int month, IEnumerable<DailyTollSummary> summaries)
    {
        ArgumentOutOfRangeException.ThrowIfEqual(vehicleId, Guid.Empty);

        if (year < 2000 || year > 2100)
            throw new ArgumentOutOfRangeException(nameof(year), year, "Year must be between 2000 and 2100.");

        if (month < 1 || month > 12)
            throw new ArgumentOutOfRangeException(nameof(month), month, "Month must be between 1 and 12.");

        List<DailyTollSummary> summaryList = summaries?.ToList()
            ?? throw new ArgumentNullException(nameof(summaries));

        if (summaryList.Count == 0)
            throw new ArgumentException("At least one daily toll summary is required.", nameof(summaries));

        return new TollInvoice(vehicleId, year, month, summaryList);
    }
}
