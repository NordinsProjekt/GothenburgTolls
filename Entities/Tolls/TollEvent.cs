using Entities.Bases;

namespace Entities.Tolls;

public class TollEvent
{
    public Guid Id { get; init; }

    public DateTime EventDateTime { get; }
    public string Zone { get; } = default!;

    public Guid? VehicleId { get; }
    public Vehicle? Vehicle { get; }

    public Guid? DailyTollSummaryId { get; }
    public DailyTollSummary? DailyTollSummaries { get; }

    //For EF
    private TollEvent() { }

    public TollEvent(DateTime eventDateTime, string zone, Guid vehicleId)
    {
        EventDateTime = eventDateTime;
        Zone = zone;
        VehicleId = vehicleId;
    }
}