namespace Entities.Tolls;

public readonly struct SwedishDateRange
{
    private static readonly TimeZoneInfo SwedishTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Europe/Stockholm");

    public DateTimeOffset Start { get; }
    public DateTimeOffset End { get; }

    public SwedishDateRange(DateOnly date)
    {
        DateTime dayStartLocal = date.ToDateTime(TimeOnly.MinValue);
        DateTime nextDayStartLocal = date.AddDays(1).ToDateTime(TimeOnly.MinValue);

        Start = new DateTimeOffset(dayStartLocal, SwedishTimeZone.GetUtcOffset(dayStartLocal));
        End = new DateTimeOffset(nextDayStartLocal, SwedishTimeZone.GetUtcOffset(nextDayStartLocal));
    }
}
