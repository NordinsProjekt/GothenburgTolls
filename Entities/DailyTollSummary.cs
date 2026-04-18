namespace Entities;

public class DailyTollSummary
{
    public Guid Id { get; set; }
    public DateTime Created { get; }
    public DateOnly ForDay { get; }
    public decimal Amount { get; }

    public Guid? TollInvoiceId { get; }
    public TollInvoice? TollInvoice { get; }

    protected List<TollEvent> TollEvents { get; } = new();

    private DailyTollSummary() { }

    public DailyTollSummary(DateOnly forDay, decimal amount)
    {
        ForDay = forDay;
        Amount = amount;
        Created = DateTime.Now;
    }
}