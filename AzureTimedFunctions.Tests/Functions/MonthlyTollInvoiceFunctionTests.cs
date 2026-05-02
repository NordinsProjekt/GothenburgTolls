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

public class MonthlyTollInvoiceFunctionTests
{
    private readonly IVehicleRepository _vehicleRepository = Substitute.For<IVehicleRepository>();
    private readonly ITollInvoiceService _tollInvoiceService = Substitute.For<ITollInvoiceService>();
    private readonly FakeLogger<MonthlyTollInvoiceFunction> _fakeLogger = new();
    private readonly MonthlyTollInvoiceFunction _sut;

    public MonthlyTollInvoiceFunctionTests()
    {
        ILoggerFactory loggerFactory = Substitute.For<ILoggerFactory>();
        loggerFactory.CreateLogger(Arg.Any<string>()).Returns(_fakeLogger);
        _sut = new MonthlyTollInvoiceFunction(_vehicleRepository, _tollInvoiceService, loggerFactory);
    }

    [Fact]
    public async Task RunAsync_WhenNoVehiclesExist_ShouldNotCallCreateAsync()
    {
        _vehicleRepository.GetAllVehicleAsync(Arg.Any<CancellationToken>()).Returns([]);

        await _sut.RunAsync(Substitute.For<TimerInfo>(), CancellationToken.None);

        await _tollInvoiceService.DidNotReceive().CreateAsync(
            Arg.Any<string>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RunAsync_WhenVehiclesExist_ShouldCallCreateAsyncForEachVehicle()
    {
        List<Vehicle> vehicles = [new Car("ABC123"), new Car("XYZ999")];
        _vehicleRepository.GetAllVehicleAsync(Arg.Any<CancellationToken>()).Returns(vehicles);

        await _sut.RunAsync(Substitute.For<TimerInfo>(), CancellationToken.None);

        await _tollInvoiceService.Received(2).CreateAsync(
            Arg.Any<string>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RunAsync_WhenVehicleExists_ShouldCallCreateAsyncWithCorrectRegistrationNumber()
    {
        const string regNr = "ABC123";
        _vehicleRepository.GetAllVehicleAsync(Arg.Any<CancellationToken>())
            .Returns([new Car(regNr)]);

        await _sut.RunAsync(Substitute.For<TimerInfo>(), CancellationToken.None);

        await _tollInvoiceService.Received(1).CreateAsync(
            regNr, Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RunAsync_WhenCreateAsyncThrowsInvalidOperationException_ShouldLogWarning()
    {
        _vehicleRepository.GetAllVehicleAsync(Arg.Any<CancellationToken>())
            .Returns([new Car("ABC123")]);
        _tollInvoiceService.CreateAsync(Arg.Any<string>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Throws(new InvalidOperationException("Already exists"));

        await _sut.RunAsync(Substitute.For<TimerInfo>(), CancellationToken.None);

        Assert.True(_fakeLogger.HasEntry(LogLevel.Warning, "Already exists"));
    }

    [Fact]
    public async Task RunAsync_WhenCreateAsyncThrowsInvalidOperationException_ShouldContinueProcessingRemainingVehicles()
    {
        List<Vehicle> vehicles = [new Car("ABC123"), new Car("XYZ999")];
        _vehicleRepository.GetAllVehicleAsync(Arg.Any<CancellationToken>()).Returns(vehicles);
        _tollInvoiceService.CreateAsync(Arg.Any<string>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Throws(new InvalidOperationException("Already exists"));

        await _sut.RunAsync(Substitute.For<TimerInfo>(), CancellationToken.None);

        await _tollInvoiceService.Received(2).CreateAsync(
            Arg.Any<string>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>());
    }
}
