namespace Entities.Tolls;

/// <summary>
/// Helper for computing Swedish-timezone-aware dates, consistent with <see cref="SwedishDateRange"/>.
/// </summary>
public static class SwedishTimeHelper
{
    private static readonly TimeZoneInfo SwedishTimeZone = GetSwedishTimeZone();

    /// <summary>Returns today's date in the Europe/Stockholm timezone.</summary>
    public static DateOnly Today()
    {
        DateTimeOffset nowSwedish = TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, SwedishTimeZone);
        return DateOnly.FromDateTime(nowSwedish.DateTime);
    }

    /// <summary>Converts a <see cref="DateTimeOffset"/> to a <see cref="DateOnly"/> in the Europe/Stockholm timezone.</summary>
    public static DateOnly ToDate(DateTimeOffset dateTimeOffset)
    {
        DateTimeOffset swedish = TimeZoneInfo.ConvertTime(dateTimeOffset, SwedishTimeZone);
        return DateOnly.FromDateTime(swedish.DateTime);
    }

    private static TimeZoneInfo GetSwedishTimeZone()
    {
        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById("Europe/Stockholm");
        }
        catch (TimeZoneNotFoundException)
        {
            return TimeZoneInfo.FindSystemTimeZoneById("Central European Standard Time");
        }
    }
}
