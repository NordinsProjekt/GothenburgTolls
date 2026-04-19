using Entities.Bases;

namespace Entities.Tolls;

public class TollEvent
{
    public Guid Id { get; init; }

    public DateTimeOffset EventDateTime { get; }
    public string Zone { get; } = default!;

    public Guid? VehicleId { get; }
    public Vehicle? Vehicle { get; }

    public Guid? DailyTollSummaryId { get; private set; }
    public DailyTollSummary? DailyTollSummaries { get; }

    //For EF
    private TollEvent() { }

    public TollEvent(DateTimeOffset eventDateTime, string zone, Guid vehicleId)
    {
        EventDateTime = eventDateTime;
        Zone = zone;
        VehicleId = vehicleId;
    }

    public void AssignToDailyTollSummary(Guid dailyTollSummaryId)
    {
        if (dailyTollSummaryId == Guid.Empty)
        {
            throw new ArgumentException("Daily toll summary id is required.", nameof(dailyTollSummaryId));
        }

        if (DailyTollSummaryId is not null)
        {
            throw new InvalidOperationException("Toll event is already assigned to a daily toll summary.");
        }

        DailyTollSummaryId = dailyTollSummaryId;
    }
}