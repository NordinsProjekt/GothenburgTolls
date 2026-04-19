namespace Entities.Tolls;

public class TollInvoice
{
    public Guid Id { get; set; }

    public DateOnly FromDay { get; }
    public DateOnly ToDay { get; }
    public decimal Sum => TollSummary.Sum(ts => ts.Amount);

    public required IReadOnlyCollection<DailyTollSummary> TollSummary { get; init; }

    private TollInvoice() { }

    public TollInvoice(DateOnly fromDay, DateOnly toDay, IEnumerable<DailyTollSummary> tollSummary)
    {
        FromDay = fromDay;
        ToDay = toDay;
        TollSummary = [.. tollSummary];
    }
}
