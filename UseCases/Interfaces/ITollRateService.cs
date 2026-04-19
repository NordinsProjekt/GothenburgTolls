namespace UseCases.Interfaces;

public interface ITollRateService
{
    int MaxDailyFee { get; }
    int GetFeeForTime(TimeOnly time);
}
