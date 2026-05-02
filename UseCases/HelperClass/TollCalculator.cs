using Entities.Interfaces;
using Entities.Types;
using UseCases.Interfaces;

namespace UseCases.HelperClass;

public class TollCalculator(ISwedishHolidayService holidayService, ITollRateService tollRateService) : ITollCalculator
{
    /// <summary>
    /// Calculate the total toll fee for one day.
    /// The maximum fee per day is configured via ITollRateService.
    /// Within a 60-minute window (inclusive) only the highest single fee is charged.
    /// </summary>
    public int CalculateDailyTotalFee(IVehicle vehicle, DateTimeOffset[] dates)
    {
        ArgumentNullException.ThrowIfNull(dates);
        if (dates.Length == 0)
        {
            return 0;
        }

        DateTimeOffset[] sortedDates = [.. dates.OrderBy(d => d)];

        if (sortedDates.Length == 1)
        {
            return Math.Min(CalculateSinglePassageFee(sortedDates[0], vehicle), tollRateService.MaxDailyFee);
        }

        DateTimeOffset intervalStart = sortedDates[0];
        int intervalHighestFee = 0;
        int totalFee = 0;

        foreach (DateTimeOffset date in sortedDates)
        {
            int currentFee = CalculateSinglePassageFee(date, vehicle);
            bool withinSameInterval = (date - intervalStart).TotalMinutes <= 60;

            if (withinSameInterval)
            {
                if (currentFee > intervalHighestFee)
                {
                    totalFee += currentFee - intervalHighestFee;
                    intervalHighestFee = currentFee;
                }
            }
            else
            {
                totalFee += currentFee;
                intervalStart = date;
                intervalHighestFee = currentFee;
            }
        }

        return Math.Min(totalFee, tollRateService.MaxDailyFee);
    }

    private static bool IsTollFreeVehicle(IVehicle vehicle)
    {
        if (vehicle is null) return false;

        return Enum.TryParse<VehicleType>(vehicle.GetVehicleType(), out VehicleType type)
               && type.IsTollFree();
    }

    private int CalculateSinglePassageFee(DateTimeOffset date, IVehicle vehicle)
    {
        if (IsTollFreeDate(date) || IsTollFreeVehicle(vehicle)) return 0;

        TimeOnly time = TimeOnly.FromDateTime(date.DateTime);
        return tollRateService.GetFeeForTime(time);
    }

    /// <summary>
    /// No toll on Saturdays, Sundays, public holidays,
    /// the day before a public holiday, or during July.
    /// </summary>
    private bool IsTollFreeDate(DateTimeOffset date)
    {
        if (date.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday) return true;
        if (date.Month == 7) return true;

        DateOnly dateOnly = DateOnly.FromDateTime(date.DateTime);
        if (holidayService.IsPublicHoliday(dateOnly)) return true;
        if (holidayService.IsDayBeforePublicHoliday(dateOnly)) return true;

        return false;
    }
}