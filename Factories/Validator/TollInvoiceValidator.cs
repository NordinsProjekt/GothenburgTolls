using Entities.Tolls;

namespace Factories.Validator;

internal static class TollInvoiceValidator
{
    internal static void ValidateVehicleId(Guid vehicleId)
    {
        ArgumentOutOfRangeException.ThrowIfEqual(vehicleId, Guid.Empty);
    }

    internal static void ValidateYear(int year)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(year);
    }

    internal static void ValidateMonth(int month)
    {
        if (month < 1 || month > 12)
            throw new ArgumentOutOfRangeException(nameof(month), month, "Month must be between 1 and 12.");
    }

    internal static List<DailyTollSummary> ValidateAndMaterializeSummaries(IEnumerable<DailyTollSummary> summaries)
    {
        List<DailyTollSummary> summaryList = summaries?.ToList()
            ?? throw new ArgumentNullException(nameof(summaries));

        if (summaryList.Count == 0)
            throw new ArgumentException("At least one daily toll summary is required.", nameof(summaries));

        return summaryList;
    }
}
