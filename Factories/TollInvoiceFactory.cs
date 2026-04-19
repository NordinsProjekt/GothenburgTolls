using Entities.Tolls;
using Factories.Validator;

namespace Factories;

public static class TollInvoiceFactory
{
    public static TollInvoice Create(Guid vehicleId, int year, int month, IEnumerable<DailyTollSummary> summaries)
    {
        TollInvoiceValidator.ValidateVehicleId(vehicleId);
        TollInvoiceValidator.ValidateYear(year);
        TollInvoiceValidator.ValidateMonth(month);
        List<DailyTollSummary> summaryList = TollInvoiceValidator.ValidateAndMaterializeSummaries(summaries);

        return new TollInvoice(vehicleId, year, month, summaryList);
    }
}
