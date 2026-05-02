using AzureTimedFunctions.Functions;
using AzureTimedFunctions.Tests.Helpers;
using Entities.Vehicels;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using NSubstitute;
using UseCases.Interfaces;
using UseCases.Results;

namespace AzureTimedFunctions.Tests.Functions;

public class BackfillDailySummaryFunctionTests
{
    private readonly IDailyTollSummaryService _dailyTollSummaryService = Substitute.For<IDailyTollSummaryService>();
    private readonly FakeLogger<BackfillDailySummaryFunction> _fakeLogger = new();
    private readonly BackfillDailySummaryFunction _sut;

    public BackfillDailySummaryFunctionTests()
    {
        ILoggerFactory loggerFactory = Substitute.For<ILoggerFactory>();
        loggerFactory.CreateLogger(Arg.Any<string>()).Returns(_fakeLogger);
        _sut = new BackfillDailySummaryFunction(_dailyTollSummaryService, loggerFactory);
    }

    [Fact]
    public async Task RunAsync_WhenCalled_ShouldCallBackfillMissedAsync()
    {
        _dailyTollSummaryService.BackfillMissedAsync(Arg.Any<CancellationToken>())
            .Returns(BackfillResult.Empty);

        await _sut.RunAsync(Substitute.For<TimerInfo>(), CancellationToken.None);

        await _dailyTollSummaryService.Received(1).BackfillMissedAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RunAsync_WhenBackfillHasFailures_ShouldLogWarning()
    {
        _dailyTollSummaryService.BackfillMissedAsync(Arg.Any<CancellationToken>())
            .Returns(new BackfillResult(Created: 0, Skipped: 0, Failed: 2));

        await _sut.RunAsync(Substitute.For<TimerInfo>(), CancellationToken.None);

        Assert.True(_fakeLogger.HasEntry(LogLevel.Warning, "2"));
    }

    [Fact]
    public async Task RunAsync_WhenBackfillHasNoFailures_ShouldNotLogWarning()
    {
        _dailyTollSummaryService.BackfillMissedAsync(Arg.Any<CancellationToken>())
            .Returns(new BackfillResult(Created: 3, Skipped: 1, Failed: 0));

        await _sut.RunAsync(Substitute.For<TimerInfo>(), CancellationToken.None);

        Assert.Equal(0, _fakeLogger.CountEntries(LogLevel.Warning));
    }

    [Fact]
    public async Task RunAsync_WhenBackfillCompletes_ShouldLogInformation()
    {
        _dailyTollSummaryService.BackfillMissedAsync(Arg.Any<CancellationToken>())
            .Returns(new BackfillResult(Created: 2, Skipped: 1, Failed: 0));

        await _sut.RunAsync(Substitute.For<TimerInfo>(), CancellationToken.None);

        Assert.True(_fakeLogger.HasEntry(LogLevel.Information, "completed"));
    }
}
