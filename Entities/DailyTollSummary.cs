using Entities.Bases;

namespace Entities;

public class DailyTollSummary
{
    public Guid Id { get; set; }

    public DateTime Created { get; }
    public DateOnly ForDay { get; }
    public decimal Amount { get; }

    public Guid VehicleId { get; init; }
    public Vehicle? Vehicle { get; init; }

    public Guid? TollInvoiceId { get; }
    public TollInvoice? TollInvoice { get; }

    public List<TollEvent> TollEvents { get; } = new();

    private DailyTollSummary() { }

    public DailyTollSummary(DateOnly forDay, decimal amount, Guid vehicleId)
    {
        ForDay = forDay;
        Amount = amount;
        Created = DateTime.Now;
        VehicleId = vehicleId;
    }
}