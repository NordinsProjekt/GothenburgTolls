namespace UseCases.Interfaces;

public interface ISwedishHolidayService
{
    bool IsPublicHoliday(DateOnly date);
    bool IsDayBeforePublicHoliday(DateOnly date);
    IReadOnlyList<DateOnly> GetPublicHolidays(int year);
}
