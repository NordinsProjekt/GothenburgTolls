using AzureTimedFunctions.Functions;
using AzureTimedFunctions.Tests.Helpers;
using Entities.Bases;
using Entities.Interfaces;
using Entities.Tolls;
using Entities.Vehicels;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using UseCases.Interfaces;

namespace AzureTimedFunctions.Tests;

public class DailyTollSummaryFunctionLoggingTests
{
    private readonly IVehicleRepository _vehicleRepository;
    private readonly IDailyTollSummaryService _dailyTollSummaryService;
    private readonly FakeLogger<DailyTollSummaryFunction> _fakeLogger;
    private readonly DailyTollSummaryFunction _function;

    private static readonly DateOnly Yesterday = DateOnly.FromDateTime(DateTime.Today.AddDays(-1));

    public DailyTollSummaryFunctionLoggingTests()
    {
        _vehicleRepository = Substitute.For<IVehicleRepository>();
        _dailyTollSummaryService = Substitute.For<IDailyTollSummaryService>();
        _fakeLogger = new FakeLogger<DailyTollSummaryFunction>();

        ILoggerFactory loggerFactory = Substitute.For<ILoggerFactory>();
        loggerFactory.CreateLogger<DailyTollSummaryFunction>().Returns(_fakeLogger);

        _function = new DailyTollSummaryFunction(_vehicleRepository, _dailyTollSummaryService, loggerFactory);
    }

    [Fact]
    public async Task RunAsync_WhenTriggered_ShouldLogInformationOnStart()
    {
        _vehicleRepository.GetAllVehicleAsync(Arg.Any<CancellationToken>()).Returns([]);

        await _function.RunAsync(Substitute.For<TimerInfo>(), CancellationToken.None);

        Assert.True(_fakeLogger.HasEntry(LogLevel.Information, "DailyTollSummaryFunction triggered"));
    }

    [Fact]
    public async Task RunAsync_WhenVehiclesFound_ShouldLogVehicleCount()
    {
        List<Vehicle> vehicles = [new Car("ABC123"), new Car("DEF456")];
        _vehicleRepository.GetAllVehicleAsync(Arg.Any<CancellationToken>()).Returns(vehicles);
        _dailyTollSummaryService.CreateAsync(Arg.Any<string>(), Arg.Any<DateOnly>(), Arg.Any<CancellationToken>())
            .Returns(new DailyTollSummary(Yesterday, 100, Guid.NewGuid()));

        await _function.RunAsync(Substitute.For<TimerInfo>(), CancellationToken.None);

        Assert.True(_fakeLogger.HasEntry(LogLevel.Information, "2"));
    }

    [Fact]
    public async Task RunAsync_WhenCreateSucceeds_ShouldLogSuccessForVehicle()
    {
        List<Vehicle> vehicles = [new Car("ABC123")];
        _vehicleRepository.GetAllVehicleAsync(Arg.Any<CancellationToken>()).Returns(vehicles);
        _dailyTollSummaryService.CreateAsync(Arg.Any<string>(), Arg.Any<DateOnly>(), Arg.Any<CancellationToken>())
            .Returns(new DailyTollSummary(Yesterday, 100, Guid.NewGuid()));

        await _function.RunAsync(Substitute.For<TimerInfo>(), CancellationToken.None);

        Assert.True(_fakeLogger.HasEntry(LogLevel.Information, "ABC123"));
    }

    [Fact]
    public async Task RunAsync_WhenCreateThrowsInvalidOperation_ShouldLogWarning()
    {
        List<Vehicle> vehicles = [new Car("ABC123")];
        _vehicleRepository.GetAllVehicleAsync(Arg.Any<CancellationToken>()).Returns(vehicles);
        _dailyTollSummaryService.CreateAsync(Arg.Any<string>(), Arg.Any<DateOnly>(), Arg.Any<CancellationToken>())
            .Throws(new InvalidOperationException("No toll events found"));

        await _function.RunAsync(Substitute.For<TimerInfo>(), CancellationToken.None);

        Assert.True(_fakeLogger.HasEntry(LogLevel.Warning, "ABC123"));
    }

    [Fact]
    public async Task RunAsync_WhenCompleted_ShouldLogSuccessCount()
    {
        List<Vehicle> vehicles = [new Car("ABC123"), new Car("DEF456")];
        _vehicleRepository.GetAllVehicleAsync(Arg.Any<CancellationToken>()).Returns(vehicles);
        _dailyTollSummaryService.CreateAsync(Arg.Any<string>(), Arg.Any<DateOnly>(), Arg.Any<CancellationToken>())
            .Returns(new DailyTollSummary(Yesterday, 100, Guid.NewGuid()));

        await _function.RunAsync(Substitute.For<TimerInfo>(), CancellationToken.None);

        Assert.True(_fakeLogger.HasEntry(LogLevel.Information, "completed"));
    }

    [Fact]
    public async Task RunAsync_WhenOneVehicleFails_ShouldLogCompletedWithFailure()
    {
        List<Vehicle> vehicles = [new Car("ABC123"), new Car("DEF456")];
        _vehicleRepository.GetAllVehicleAsync(Arg.Any<CancellationToken>()).Returns(vehicles);
        _dailyTollSummaryService.CreateAsync("ABC123", Arg.Any<DateOnly>(), Arg.Any<CancellationToken>())
            .Throws(new InvalidOperationException("No events"));
        _dailyTollSummaryService.CreateAsync("DEF456", Arg.Any<DateOnly>(), Arg.Any<CancellationToken>())
            .Returns(new DailyTollSummary(Yesterday, 50, Guid.NewGuid()));

        await _function.RunAsync(Substitute.For<TimerInfo>(), CancellationToken.None);

        Assert.Equal(1, _fakeLogger.CountEntries(LogLevel.Warning));
    }

    [Fact]
    public async Task RunAsync_WhenNoVehicles_ShouldLogZeroCount()
    {
        _vehicleRepository.GetAllVehicleAsync(Arg.Any<CancellationToken>()).Returns([]);

        await _function.RunAsync(Substitute.For<TimerInfo>(), CancellationToken.None);

        Assert.True(_fakeLogger.HasEntry(LogLevel.Information, "0"));
    }
}
