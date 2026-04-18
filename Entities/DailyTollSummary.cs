namespace Entities;

public class DailyTollSummary(DateOnly forDay, decimal amount)
{
    public Guid Id { get; set; }
    public DateTime Created { get; }
    public DateOnly ForDay { get; } = forDay;
    public decimal Amount { get; set; } = amount;

    protected List<TollEvent> TollEvents { get; set; } = new();
}