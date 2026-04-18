namespace Entities;

public class TollInvoice
{
    public Guid Id { get; }

    public DateOnly FromDay { get; }
    public DateOnly ToDay { get; }
    public decimal Sum => TollSummary.Sum(ts => ts.Amount);

    protected List<DailyTollSummary> TollSummary { get; } = new();

    private TollInvoice() { }

    public TollInvoice(DateOnly fromDay, DateOnly toDay, IEnumerable<DailyTollSummary> tollSummary)
    {
        FromDay = fromDay;
        ToDay = toDay;
        TollSummary.AddRange(tollSummary);
    }

}
