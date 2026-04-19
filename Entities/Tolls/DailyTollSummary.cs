using Entities.Bases;

namespace Entities.Tolls;

public class DailyTollSummary
{
    public Guid Id { get; init; }

    public DateTime Created { get; }
    public DateOnly ForDay { get; }
    public decimal Amount { get; }

    public Guid VehicleId { get; init; }
    public Vehicle? Vehicle { get; init; }

    public Guid? TollInvoiceId { get; private set; }
    public TollInvoice? TollInvoice { get; }

    public void AssignToTollInvoice(Guid tollInvoiceId)
    {
        if (tollInvoiceId == Guid.Empty)
            throw new ArgumentException("Toll invoice id is required.", nameof(tollInvoiceId));

        if (TollInvoiceId is not null)
            throw new InvalidOperationException("Daily toll summary is already assigned to a toll invoice.");

        TollInvoiceId = tollInvoiceId;
    }

    public List<TollEvent> TollEvents { get; } = new();

    private DailyTollSummary() { }

    public DailyTollSummary(DateOnly forDay, decimal amount, Guid vehicleId)
    {
        Id = Guid.NewGuid();
        ForDay = forDay;
        Amount = amount;
        Created = DateTime.Now;
        VehicleId = vehicleId;
    }
}