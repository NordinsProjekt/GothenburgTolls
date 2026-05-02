using AzureTimedFunctions.Functions;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using UseCases.Interfaces;
using UseCases.Results;

namespace AzureTimedFunctions.Tests;

public class BackfillDailySummaryFunctionTests
{
    private readonly IDailyTollSummaryService _dailyTollSummaryService;
    private readonly ILoggerFactory _loggerFactory;
    private readonly BackfillDailySummaryFunction _function;

    public BackfillDailySummaryFunctionTests()
    {
        _dailyTollSummaryService = Substitute.For<IDailyTollSummaryService>();
        _loggerFactory = Substitute.For<ILoggerFactory>();
        _loggerFactory.CreateLogger<BackfillDailySummaryFunction>()
            .Returns(Substitute.For<ILogger<BackfillDailySummaryFunction>>());

        _function = new BackfillDailySummaryFunction(_dailyTollSummaryService, _loggerFactory);
    }

    [Fact]
    public async Task RunAsync_WhenCalled_ShouldCallBackfillMissedAsync()
    {
        _dailyTollSummaryService.BackfillMissedAsync(Arg.Any<CancellationToken>())
            .Returns(BackfillResult.Empty);

        await _function.RunAsync(Substitute.For<TimerInfo>(), CancellationToken.None);

        await _dailyTollSummaryService.Received(1).BackfillMissedAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RunAsync_WhenBackfillSucceeds_ShouldNotThrow()
    {
        _dailyTollSummaryService.BackfillMissedAsync(Arg.Any<CancellationToken>())
            .Returns(new BackfillResult(5, 2, 0));

        var exception = await Record.ExceptionAsync(async () =>
            await _function.RunAsync(Substitute.For<TimerInfo>(), CancellationToken.None));

        Assert.Null(exception);
    }

    [Fact]
    public async Task RunAsync_WhenBackfillThrows_ShouldPropagateException()
    {
        _dailyTollSummaryService.BackfillMissedAsync(Arg.Any<CancellationToken>())
            .Throws(new InvalidOperationException("Unexpected failure"));

        var exception = await Record.ExceptionAsync(async () =>
            await _function.RunAsync(Substitute.For<TimerInfo>(), CancellationToken.None));

        Assert.IsType<InvalidOperationException>(exception);
    }
}
