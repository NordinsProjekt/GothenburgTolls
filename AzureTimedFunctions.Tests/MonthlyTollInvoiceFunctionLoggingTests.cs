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

public class MonthlyTollInvoiceFunctionLoggingTests
{
    private readonly IVehicleRepository _vehicleRepository;
    private readonly ITollInvoiceService _tollInvoiceService;
    private readonly FakeLogger<MonthlyTollInvoiceFunction> _fakeLogger;
    private readonly MonthlyTollInvoiceFunction _function;

    private static readonly DateTime PreviousMonth = DateTime.Today.AddMonths(-1);

    public MonthlyTollInvoiceFunctionLoggingTests()
    {
        _vehicleRepository = Substitute.For<IVehicleRepository>();
        _tollInvoiceService = Substitute.For<ITollInvoiceService>();
        _fakeLogger = new FakeLogger<MonthlyTollInvoiceFunction>();

        ILoggerFactory loggerFactory = Substitute.For<ILoggerFactory>();
        loggerFactory.CreateLogger<MonthlyTollInvoiceFunction>().Returns(_fakeLogger);

        _function = new MonthlyTollInvoiceFunction(_vehicleRepository, _tollInvoiceService, loggerFactory);
    }

    [Fact]
    public async Task RunAsync_WhenTriggered_ShouldLogInformationOnStart()
    {
        _vehicleRepository.GetAllVehicleAsync(Arg.Any<CancellationToken>()).Returns([]);

        await _function.RunAsync(Substitute.For<TimerInfo>(), CancellationToken.None);

        Assert.True(_fakeLogger.HasEntry(LogLevel.Information, "MonthlyTollInvoiceFunction triggered"));
    }

    [Fact]
    public async Task RunAsync_WhenTriggered_ShouldLogPreviousYear()
    {
        _vehicleRepository.GetAllVehicleAsync(Arg.Any<CancellationToken>()).Returns([]);

        await _function.RunAsync(Substitute.For<TimerInfo>(), CancellationToken.None);

        Assert.True(_fakeLogger.HasEntry(LogLevel.Information, PreviousMonth.Year.ToString()));
    }

    [Fact]
    public async Task RunAsync_WhenTriggered_ShouldLogPreviousMonth()
    {
        _vehicleRepository.GetAllVehicleAsync(Arg.Any<CancellationToken>()).Returns([]);

        await _function.RunAsync(Substitute.For<TimerInfo>(), CancellationToken.None);

        Assert.True(_fakeLogger.HasEntry(LogLevel.Information, $"{PreviousMonth.Year}-{PreviousMonth.Month:D2}"));
    }

    [Fact]
    public async Task RunAsync_WhenVehiclesFound_ShouldLogVehicleCount()
    {
        List<Vehicle> vehicles = [new Car("ABC123"), new Car("DEF456")];
        _vehicleRepository.GetAllVehicleAsync(Arg.Any<CancellationToken>()).Returns(vehicles);
        _tollInvoiceService.CreateAsync(Arg.Any<string>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(new TollInvoice(Guid.NewGuid(), PreviousMonth.Year, PreviousMonth.Month, []));

        await _function.RunAsync(Substitute.For<TimerInfo>(), CancellationToken.None);

        Assert.True(_fakeLogger.HasEntry(LogLevel.Information, "2"));
    }

    [Fact]
    public async Task RunAsync_WhenCreateSucceeds_ShouldLogSuccessForVehicle()
    {
        List<Vehicle> vehicles = [new Car("ABC123")];
        _vehicleRepository.GetAllVehicleAsync(Arg.Any<CancellationToken>()).Returns(vehicles);
        _tollInvoiceService.CreateAsync(Arg.Any<string>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(new TollInvoice(Guid.NewGuid(), PreviousMonth.Year, PreviousMonth.Month, []));

        await _function.RunAsync(Substitute.For<TimerInfo>(), CancellationToken.None);

        Assert.True(_fakeLogger.HasEntry(LogLevel.Information, "ABC123"));
    }

    [Fact]
    public async Task RunAsync_WhenCreateThrowsInvalidOperation_ShouldLogWarning()
    {
        List<Vehicle> vehicles = [new Car("ABC123")];
        _vehicleRepository.GetAllVehicleAsync(Arg.Any<CancellationToken>()).Returns(vehicles);
        _tollInvoiceService.CreateAsync(Arg.Any<string>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Throws(new InvalidOperationException("No uninvoiced summaries found"));

        await _function.RunAsync(Substitute.For<TimerInfo>(), CancellationToken.None);

        Assert.True(_fakeLogger.HasEntry(LogLevel.Warning, "ABC123"));
    }

    [Fact]
    public async Task RunAsync_WhenCompleted_ShouldLogCompletedMessage()
    {
        List<Vehicle> vehicles = [new Car("ABC123")];
        _vehicleRepository.GetAllVehicleAsync(Arg.Any<CancellationToken>()).Returns(vehicles);
        _tollInvoiceService.CreateAsync(Arg.Any<string>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(new TollInvoice(Guid.NewGuid(), PreviousMonth.Year, PreviousMonth.Month, []));

        await _function.RunAsync(Substitute.For<TimerInfo>(), CancellationToken.None);

        Assert.True(_fakeLogger.HasEntry(LogLevel.Information, "completed"));
    }

    [Fact]
    public async Task RunAsync_WhenAllVehiclesFail_ShouldOnlyLogWarnings()
    {
        List<Vehicle> vehicles = [new Car("ABC123"), new Car("DEF456")];
        _vehicleRepository.GetAllVehicleAsync(Arg.Any<CancellationToken>()).Returns(vehicles);
        _tollInvoiceService.CreateAsync(Arg.Any<string>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Throws(new InvalidOperationException("No uninvoiced summaries found"));

        await _function.RunAsync(Substitute.For<TimerInfo>(), CancellationToken.None);

        Assert.Equal(2, _fakeLogger.CountEntries(LogLevel.Warning));
    }
}
