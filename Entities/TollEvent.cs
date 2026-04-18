using Entities.Bases;

namespace Entities;

public class TollEvent
{
    private readonly Guid vehicle;

    public Guid Id { get; set; }

    public DateTime EventDateTime { get; set; }

    public Guid? VehicleId { get; set; }
    public Vehicle? Vehicle { get; set; }

    public Guid? DailyTollSummaryId { get; set; }
    public DailyTollSummary? DailyTollSummary { get; set; }

    //For EF
    private TollEvent() { }

    public TollEvent(DateTime eventDateTime, Guid vehicleId)
    {
        EventDateTime = eventDateTime;
        VehicleId = vehicleId;
    }
}