using Entities.Bases;

namespace Entities.Tolls;

public class TollInvoice
{
    public Guid Id { get; init; }

    public Guid VehicleId { get; }
    public Vehicle? Vehicle { get; }

    public int Year { get; }
    public int Month { get; }
    public decimal Sum => TollSummary.Sum(ts => ts.Amount);

    public DateTime Created { get; }

    public IReadOnlyCollection<DailyTollSummary> TollSummary { get; init; } = new List<DailyTollSummary>();

    private TollInvoice() { }

    public TollInvoice(Guid vehicleId, int year, int month, IEnumerable<DailyTollSummary> tollSummary)
    {
        if (vehicleId == Guid.Empty)
            throw new ArgumentException("Vehicle id is required.", nameof(vehicleId));
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(year);
        if (month < 1 || month > 12)
            throw new ArgumentOutOfRangeException(nameof(month), month, "Month must be between 1 and 12.");

        Id = Guid.NewGuid();
        VehicleId = vehicleId;
        Year = year;
        Month = month;
        Created = DateTime.Now;
        TollSummary = new List<DailyTollSummary>(tollSummary);
    }
}
