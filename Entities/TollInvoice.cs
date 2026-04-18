namespace Entities;

public class TollInvoice
{
    public Guid Id { get; }

    public DateOnly FromDay { get; }
    public DateOnly ToDay { get; }
    protected decimal Sum => TollSummary.Sum(ts => ts.Amount);

    protected List<DailyTollSummary> TollSummary { get; set; } = new();

}
