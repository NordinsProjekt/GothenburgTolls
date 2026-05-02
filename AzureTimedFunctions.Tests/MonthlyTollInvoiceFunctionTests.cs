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

public class MonthlyTollInvoiceFunctionTests
{
    private readonly IVehicleRepository _vehicleRepository;
    private readonly ITollInvoiceService _tollInvoiceService;
    private readonly ILoggerFactory _loggerFactory;
    private readonly MonthlyTollInvoiceFunction _function;

    public MonthlyTollInvoiceFunctionTests()
    {
        _vehicleRepository = Substitute.For<IVehicleRepository>();
        _tollInvoiceService = Substitute.For<ITollInvoiceService>();
        _loggerFactory = Substitute.For<ILoggerFactory>();
        _loggerFactory.CreateLogger<MonthlyTollInvoiceFunction>()
            .Returns(Substitute.For<ILogger<MonthlyTollInvoiceFunction>>());

        _function = new MonthlyTollInvoiceFunction(
            _vehicleRepository,
            _tollInvoiceService,
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
        _tollInvoiceService.CreateAsync(Arg.Any<string>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(new TollInvoice(Guid.NewGuid(), 2025, 1, []));

        await _function.RunAsync(Substitute.For<TimerInfo>(), CancellationToken.None);

        await _tollInvoiceService.Received(2).CreateAsync(
            Arg.Any<string>(),
            Arg.Any<int>(),
            Arg.Any<int>(),
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
        _tollInvoiceService.CreateAsync("ABC123", Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Throws(new InvalidOperationException("No uninvoiced summaries found"));
        _tollInvoiceService.CreateAsync("DEF456", Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(new TollInvoice(Guid.NewGuid(), 2025, 1, []));

        await _function.RunAsync(Substitute.For<TimerInfo>(), CancellationToken.None);

        await _tollInvoiceService.Received(1).CreateAsync(
            "DEF456",
            Arg.Any<int>(),
            Arg.Any<int>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RunAsync_WhenCreateThrowsInvalidOperation_ShouldNotThrow()
    {
        List<Vehicle> vehicles = [new Car("ABC123")];
        _vehicleRepository.GetAllVehicleAsync(Arg.Any<CancellationToken>())
            .Returns(vehicles);
        _tollInvoiceService.CreateAsync(Arg.Any<string>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Throws(new InvalidOperationException("No uninvoiced summaries found"));

        var exception = await Record.ExceptionAsync(async () =>
            await _function.RunAsync(Substitute.For<TimerInfo>(), CancellationToken.None));

        Assert.Null(exception);
    }
}
