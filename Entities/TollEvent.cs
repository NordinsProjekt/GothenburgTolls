using Entities.Bases;

namespace Entities;

public class TollEvent(DateTime eventDateTime, Guid Vehicle)
{
    public Guid Id { get; set; }

    public DateTime EventDateTime { get; set; } = eventDateTime;

    public Guid? VehicleId { get; set; } = Vehicle;
    public Vehicle? Vehicle { get; set; }

    public Guid? DailyTollSummaryId { get; set; }
    public DailyTollSummary? DailyTollSummary { get; set; }
}