using AzureTimedFunctions.Functions;
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

public class DailyTollSummaryFunctionTests
{
    private readonly IVehicleRepository _vehicleRepository;
    private readonly IDailyTollSummaryService _dailyTollSummaryService;
    private readonly ILoggerFactory _loggerFactory;
    private readonly DailyTollSummaryFunction _function;

    public DailyTollSummaryFunctionTests()
    {
        _vehicleRepository = Substitute.For<IVehicleRepository>();
        _dailyTollSummaryService = Substitute.For<IDailyTollSummaryService>();
        _loggerFactory = Substitute.For<ILoggerFactory>();
        _loggerFactory.CreateLogger<DailyTollSummaryFunction>()
            .Returns(Substitute.For<ILogger<DailyTollSummaryFunction>>());

        _function = new DailyTollSummaryFunction(
            _vehicleRepository,
            _dailyTollSummaryService,
            _loggerFactory);
    }

    [Fact]
    public async Task RunAsync_WhenVehiclesExist_ShouldCallCreateAsyncForEachVehicle()
    {
        List<Vehicle> vehicles = [
            new Car("ABC123"),
            new Car("DEF456")
        ];
        _vehicleRepository.GetAllVehicleAsync(Arg.Any<CancellationToken>())
            .Returns(vehicles);
        _dailyTollSummaryService.CreateAsync(Arg.Any<string>(), Arg.Any<DateOnly>(), Arg.Any<CancellationToken>())
            .Returns(new DailyTollSummary(DateOnly.FromDateTime(DateTime.Today), 100, Guid.NewGuid()));

        await _function.RunAsync(Substitute.For<TimerInfo>(), CancellationToken.None);

        await _dailyTollSummaryService.Received(2).CreateAsync(
            Arg.Any<string>(),
            Arg.Any<DateOnly>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RunAsync_WhenCreateThrowsInvalidOperation_ShouldContinueToNextVehicle()
    {
        List<Vehicle> vehicles = [
            new Car("ABC123"),
            new Car("DEF456")
        ];
        _vehicleRepository.GetAllVehicleAsync(Arg.Any<CancellationToken>())
            .Returns(vehicles);
        _dailyTollSummaryService.CreateAsync("ABC123", Arg.Any<DateOnly>(), Arg.Any<CancellationToken>())
            .Throws(new InvalidOperationException("No toll events found"));
        _dailyTollSummaryService.CreateAsync("DEF456", Arg.Any<DateOnly>(), Arg.Any<CancellationToken>())
            .Returns(new DailyTollSummary(DateOnly.FromDateTime(DateTime.Today), 100, Guid.NewGuid()));

        await _function.RunAsync(Substitute.For<TimerInfo>(), CancellationToken.None);

        await _dailyTollSummaryService.Received(1).CreateAsync(
            "DEF456",
            Arg.Any<DateOnly>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RunAsync_WhenCreateThrowsInvalidOperation_ShouldNotThrow()
    {
        List<Vehicle> vehicles = [new Car("ABC123")];
        _vehicleRepository.GetAllVehicleAsync(Arg.Any<CancellationToken>())
            .Returns(vehicles);
        _dailyTollSummaryService.CreateAsync(Arg.Any<string>(), Arg.Any<DateOnly>(), Arg.Any<CancellationToken>())
            .Throws(new InvalidOperationException("No toll events found"));

        var exception = await Record.ExceptionAsync(async () =>
            await _function.RunAsync(Substitute.For<TimerInfo>(), CancellationToken.None));

        Assert.Null(exception);
    }
}
