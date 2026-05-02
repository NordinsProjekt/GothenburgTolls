using AzureTimedFunctions.Functions;
using AzureTimedFunctions.Tests.Helpers;
using Entities.Bases;
using Entities.Interfaces;
using Entities.Vehicels;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using UseCases.Interfaces;

namespace AzureTimedFunctions.Tests.Functions;

public class DailyTollSummaryFunctionTests
{
    private readonly IVehicleRepository _vehicleRepository = Substitute.For<IVehicleRepository>();
    private readonly IDailyTollSummaryService _dailyTollSummaryService = Substitute.For<IDailyTollSummaryService>();
    private readonly FakeLogger<DailyTollSummaryFunction> _fakeLogger = new();
    private readonly DailyTollSummaryFunction _sut;

    public DailyTollSummaryFunctionTests()
    {
        ILoggerFactory loggerFactory = Substitute.For<ILoggerFactory>();
        loggerFactory.CreateLogger(Arg.Any<string>()).Returns(_fakeLogger);
        _sut = new DailyTollSummaryFunction(_vehicleRepository, _dailyTollSummaryService, loggerFactory);
    }

    [Fact]
    public async Task RunAsync_WhenNoVehiclesExist_ShouldNotCallCreateAsync()
    {
        _vehicleRepository.GetAllVehicleAsync(Arg.Any<CancellationToken>())
            .Returns([]);

        await _sut.RunAsync(Substitute.For<TimerInfo>(), CancellationToken.None);

        await _dailyTollSummaryService.DidNotReceive().CreateAsync(
            Arg.Any<string>(), Arg.Any<DateOnly>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RunAsync_WhenVehiclesExist_ShouldCallCreateAsyncForEachVehicle()
    {
        List<Vehicle> vehicles = [CreateCar("ABC123"), CreateCar("XYZ999")];
        _vehicleRepository.GetAllVehicleAsync(Arg.Any<CancellationToken>()).Returns(vehicles);

        await _sut.RunAsync(Substitute.For<TimerInfo>(), CancellationToken.None);

        await _dailyTollSummaryService.Received(2).CreateAsync(
            Arg.Any<string>(), Arg.Any<DateOnly>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RunAsync_WhenVehicleExists_ShouldCallCreateAsyncWithCorrectRegistrationNumber()
    {
        const string regNr = "ABC123";
        _vehicleRepository.GetAllVehicleAsync(Arg.Any<CancellationToken>())
            .Returns([CreateCar(regNr)]);

        await _sut.RunAsync(Substitute.For<TimerInfo>(), CancellationToken.None);

        await _dailyTollSummaryService.Received(1).CreateAsync(
            regNr, Arg.Any<DateOnly>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RunAsync_WhenCreateAsyncThrowsInvalidOperationException_ShouldLogWarning()
    {
        _vehicleRepository.GetAllVehicleAsync(Arg.Any<CancellationToken>())
            .Returns([CreateCar("ABC123")]);
        _dailyTollSummaryService.CreateAsync(Arg.Any<string>(), Arg.Any<DateOnly>(), Arg.Any<CancellationToken>())
            .Throws(new InvalidOperationException("Already exists"));

        await _sut.RunAsync(Substitute.For<TimerInfo>(), CancellationToken.None);

        Assert.True(_fakeLogger.HasEntry(LogLevel.Warning, "Already exists"));
    }

    [Fact]
    public async Task RunAsync_WhenCreateAsyncThrowsInvalidOperationException_ShouldContinueProcessingRemainingVehicles()
    {
        List<Vehicle> vehicles = [CreateCar("ABC123"), CreateCar("XYZ999")];
        _vehicleRepository.GetAllVehicleAsync(Arg.Any<CancellationToken>()).Returns(vehicles);
        _dailyTollSummaryService.CreateAsync(Arg.Any<string>(), Arg.Any<DateOnly>(), Arg.Any<CancellationToken>())
            .Throws(new InvalidOperationException("Already exists"));

        await _sut.RunAsync(Substitute.For<TimerInfo>(), CancellationToken.None);

        await _dailyTollSummaryService.Received(2).CreateAsync(
            Arg.Any<string>(), Arg.Any<DateOnly>(), Arg.Any<CancellationToken>());
    }

    private static Car CreateCar(string registrationNumber) => new(registrationNumber);
}
