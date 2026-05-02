using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using UseCases.Interfaces;
using UseCases.Results;

namespace AzureTimedFunctions.Functions;

public class BackfillDailySummaryFunction(
    IDailyTollSummaryService dailyTollSummaryService,
    ILoggerFactory loggerFactory)
{
    private readonly ILogger _logger = loggerFactory.CreateLogger<BackfillDailySummaryFunction>();

    [Function(nameof(BackfillDailySummaryFunction))]
    public async Task RunAsync(
        [TimerTrigger("0 30 1 * * *")] TimerInfo timerInfo,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("BackfillDailySummaryFunction triggered at: {Now}", DateTime.UtcNow);

        BackfillResult result = await dailyTollSummaryService.BackfillMissedAsync(cancellationToken);

        _logger.LogInformation(
            "BackfillDailySummaryFunction completed. Created: {Created}, Skipped (already exists or no events): {Skipped}, Failed: {Failed}",
            result.Created,
            result.Skipped,
            result.Failed);

        if (result.Failed > 0)
        {
            _logger.LogWarning("BackfillDailySummaryFunction encountered {Failed} unexpected failures.", result.Failed);
        }
    }
}
