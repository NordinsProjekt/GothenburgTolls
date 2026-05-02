using AzureTimedFunctions.Functions;
using AzureTimedFunctions.Tests.Helpers;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using UseCases.Interfaces;
using UseCases.Results;

namespace AzureTimedFunctions.Tests;

public class BackfillDailySummaryFunctionLoggingTests
{
    private readonly IDailyTollSummaryService _dailyTollSummaryService;
    private readonly FakeLogger<BackfillDailySummaryFunction> _fakeLogger;
    private readonly BackfillDailySummaryFunction _function;

    public BackfillDailySummaryFunctionLoggingTests()
    {
        _dailyTollSummaryService = Substitute.For<IDailyTollSummaryService>();
        _fakeLogger = new FakeLogger<BackfillDailySummaryFunction>();

        ILoggerFactory loggerFactory = Substitute.For<ILoggerFactory>();
        loggerFactory.CreateLogger<BackfillDailySummaryFunction>().Returns(_fakeLogger);

        _function = new BackfillDailySummaryFunction(_dailyTollSummaryService, loggerFactory);
    }

    [Fact]
    public async Task RunAsync_WhenTriggered_ShouldLogInformationOnStart()
    {
        _dailyTollSummaryService.BackfillMissedAsync(Arg.Any<CancellationToken>())
            .Returns(BackfillResult.Empty);

        await _function.RunAsync(Substitute.For<TimerInfo>(), CancellationToken.None);

        Assert.True(_fakeLogger.HasEntry(LogLevel.Information, "BackfillDailySummaryFunction triggered"));
    }

    [Fact]
    public async Task RunAsync_WhenBackfillCompletes_ShouldLogCompletedMessage()
    {
        _dailyTollSummaryService.BackfillMissedAsync(Arg.Any<CancellationToken>())
            .Returns(new BackfillResult(3, 1, 0));

        await _function.RunAsync(Substitute.For<TimerInfo>(), CancellationToken.None);

        Assert.True(_fakeLogger.HasEntry(LogLevel.Information, "completed"));
    }

    [Fact]
    public async Task RunAsync_WhenBackfillCreatesEntries_ShouldLogCreatedCount()
    {
        _dailyTollSummaryService.BackfillMissedAsync(Arg.Any<CancellationToken>())
            .Returns(new BackfillResult(5, 0, 0));

        await _function.RunAsync(Substitute.For<TimerInfo>(), CancellationToken.None);

        Assert.True(_fakeLogger.HasEntry(LogLevel.Information, "5"));
    }

    [Fact]
    public async Task RunAsync_WhenBackfillHasFailures_ShouldLogWarning()
    {
        _dailyTollSummaryService.BackfillMissedAsync(Arg.Any<CancellationToken>())
            .Returns(new BackfillResult(2, 1, 3));

        await _function.RunAsync(Substitute.For<TimerInfo>(), CancellationToken.None);

        Assert.True(_fakeLogger.HasEntry(LogLevel.Warning, "3"));
    }

    [Fact]
    public async Task RunAsync_WhenBackfillHasNoFailures_ShouldNotLogWarning()
    {
        _dailyTollSummaryService.BackfillMissedAsync(Arg.Any<CancellationToken>())
            .Returns(new BackfillResult(2, 1, 0));

        await _function.RunAsync(Substitute.For<TimerInfo>(), CancellationToken.None);

        Assert.Equal(0, _fakeLogger.CountEntries(LogLevel.Warning));
    }

    [Fact]
    public async Task RunAsync_WhenBackfillReturnsEmpty_ShouldNotLogWarning()
    {
        _dailyTollSummaryService.BackfillMissedAsync(Arg.Any<CancellationToken>())
            .Returns(BackfillResult.Empty);

        await _function.RunAsync(Substitute.For<TimerInfo>(), CancellationToken.None);

        Assert.Equal(0, _fakeLogger.CountEntries(LogLevel.Warning));
    }
}
