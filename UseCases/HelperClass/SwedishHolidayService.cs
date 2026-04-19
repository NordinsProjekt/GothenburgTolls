using PublicHoliday;
using UseCases.Interfaces;

namespace UseCases.HelperClass;

public class SwedishHolidayService : ISwedishHolidayService
{
    private readonly SwedenPublicHoliday _calendar = new();

    public bool IsPublicHoliday(DateOnly date)
    {
        return _calendar.IsPublicHoliday(date.ToDateTime(TimeOnly.MinValue));
    }

    public bool IsDayBeforePublicHoliday(DateOnly date)
    {
        DateOnly nextDay = date.AddDays(1);
        return _calendar.IsPublicHoliday(nextDay.ToDateTime(TimeOnly.MinValue));
    }

    public IReadOnlyList<DateOnly> GetPublicHolidays(int year)
    {
        return _calendar.PublicHolidays(year)
            .Select(DateOnly.FromDateTime)
            .Order()
            .ToList();
    }
}
