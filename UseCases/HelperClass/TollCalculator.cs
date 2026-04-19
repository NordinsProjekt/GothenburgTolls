using Entities.Interfaces;
using Entities.Types;
using UseCases.Interfaces;

namespace UseCases.HelperClass;

public class TollCalculator(ISwedishHolidayService holidayService)
{
    /// <summary>
    /// Calculate the total toll fee for one day.
    /// The maximum fee per day is 60 SEK.
    /// Within a 60-minute window only the highest single fee is charged.
    /// </summary>
    public int CalculateDailyTotalFee(IVehicle vehicle, DateTime[] dates)
    {
        ArgumentNullException.ThrowIfNull(dates);
        if (dates.Length == 0) return 0;

        if (dates.Length == 1) return CalculateSinglePassageFee(dates[0], vehicle);

        DateTime intervalStart = dates[0];
        int intervalHighestFee = 0;
        int totalFee = 0;

        foreach (DateTime date in dates)
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

        return Math.Min(totalFee, 60);
    }

    private bool IsTollFreeVehicle(IVehicle vehicle)
    {
        if (vehicle is null) return false;

        return Enum.TryParse<VehicleType>(vehicle.GetVehicleType(), out VehicleType type)
               && type.IsTollFree();
    }

    private static readonly (TimeOnly From, TimeOnly To, int Fee)[] TollRates =
    [
        (new(6, 0),  new(6, 30),  8),
        (new(6, 30), new(7, 0),   13),
        (new(7, 0),  new(8, 0),   18),
        (new(8, 0),  new(8, 30),  13),
        (new(8, 30), new(15, 0),  8),
        (new(15, 0), new(15, 30), 13),
        (new(15, 30),new(17, 0),  18),
        (new(17, 0), new(18, 0),  13),
        (new(18, 0), new(18, 30), 8)
    ];

    private int CalculateSinglePassageFee(DateTime date, IVehicle vehicle)
    {
        if (IsTollFreeDate(date) || IsTollFreeVehicle(vehicle)) return 0;

        TimeOnly time = TimeOnly.FromDateTime(date);

        foreach ((TimeOnly from, TimeOnly to, int fee) in TollRates)
        {
            if (time >= from && time < to) return fee;
        }

        return 0;
    }

    /// <summary>
    /// No toll on Saturdays, Sundays, public holidays,
    /// the day before a public holiday, or during July.
    /// </summary>
    private bool IsTollFreeDate(DateTime date)
    {
        if (date.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday) return true;
        if (date.Month == 7) return true;

        DateOnly dateOnly = DateOnly.FromDateTime(date);
        if (holidayService.IsPublicHoliday(dateOnly)) return true;
        if (holidayService.IsDayBeforePublicHoliday(dateOnly)) return true;

        return false;
    }
}