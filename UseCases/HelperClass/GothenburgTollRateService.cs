using UseCases.Interfaces;

namespace UseCases.HelperClass;

public class GothenburgTollRateService : ITollRateService
{
    public int MaxDailyFee => 60;

    private static readonly (TimeOnly From, TimeOnly To, int Fee)[] TollRates =
    [
        (new(6, 0),   new(6, 30),  8),
        (new(6, 30),  new(7, 0),   13),
        (new(7, 0),   new(8, 0),   18),
        (new(8, 0),   new(8, 30),  13),
        (new(8, 30),  new(15, 0),  8),
        (new(15, 0),  new(15, 30), 13),
        (new(15, 30), new(17, 0),  18),
        (new(17, 0),  new(18, 0),  13),
        (new(18, 0),  new(18, 30), 8),
    ];

    public int GetFeeForTime(TimeOnly time)
    {
        foreach ((TimeOnly from, TimeOnly to, int fee) in TollRates)
        {
            if (time >= from && time < to) return fee;
        }

        return 0;
    }
}
