using Entities.Bases;

namespace Entities;

public class TollEvent
{
    public Guid Id { get; set; }

    public DateTime EventDateTime { get; }

    public Guid? VehicleId { get; }
    public Vehicle? Vehicle { get; }

    public Guid? DailyTollSummaryId { get; }
    public DailyTollSummary? DailyTollSummaries { get; }

    //For EF
    private TollEvent() { }

    public TollEvent(DateTime eventDateTime, Guid vehicleId)
    {
        EventDateTime = eventDateTime;
        VehicleId = vehicleId;
    }
}