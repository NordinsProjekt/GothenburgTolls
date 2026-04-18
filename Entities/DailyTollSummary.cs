namespace Entities;

public class DailyTollSummary(DateOnly forDay, decimal amount)
{
    public Guid Id { get; set; }
    public DateTime Created { get; }
    public DateOnly ForDay { get; } = forDay;
    public decimal Amount { get; set; } = amount;

    public Guid? TollInvoiceId { get; set; }
    public TollInvoice? TollInvoice { get; set; }

    protected List<TollEvent> TollEvents { get; set; } = new();
}