namespace Entities;

public class DailyTollSummary
{
    public Guid Id { get; set; }
    public DateTime Created { get; }
    public DateOnly ForDay { get; }
    public decimal Amount { get; }

    public Guid? TollInvoiceId { get; set; }
    public TollInvoice? TollInvoice { get; set; }

    protected List<TollEvent> TollEvents { get; set; } = new();

    private DailyTollSummary() { }

    public DailyTollSummary(DateOnly forDay, decimal amount)
    {
        ForDay = forDay;
        Amount = amount;
        Created = DateTime.Now;
    }
}