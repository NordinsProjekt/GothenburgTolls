using Entities.Interfaces;
using Entities.Tolls;
using Entities.Vehicels;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using UseCases.Interfaces;
using UseCases.Results;
using UseCases.Services;

namespace Application.UseCases.Tests;

public class DailyTollSummaryServiceBackfillTests
{
    private readonly IVehicleRepository _vehicleRepository = Substitute.For<IVehicleRepository>();
    private readonly ITollEventRepository _tollEventRepository = Substitute.For<ITollEventRepository>();
    private readonly IDailyTollSummaryRepository _dailyTollSummaryRepository = Substitute.For<IDailyTollSummaryRepository>();
    private readonly ITollCalculator _tollCalculator = Substitute.For<ITollCalculator>();
    private readonly DailyTollSummaryService _sut;

    private static readonly DateOnly Today = DateOnly.FromDateTime(DateTime.Today);
    private static readonly DateOnly Yesterday = Today.AddDays(-1);

    private static Car CreateVehicle(string registrationNumber = "ABC123") =>
        new Car(registrationNumber) { Id = Guid.NewGuid() };

    public DailyTollSummaryServiceBackfillTests()
    {
        _sut = new DailyTollSummaryService(
            _vehicleRepository,
            _tollEventRepository,
            _dailyTollSummaryRepository,
            _tollCalculator);
    }

    [Fact]
    public async Task BackfillMissedAsync_WhenNoUnassignedEvents_ShouldReturnZeroCreated()
    {
        _tollEventRepository.GetUnassignedBeforeDateAsync(Arg.Any<DateOnly>(), Arg.Any<CancellationToken>())
            .Returns([]);

        BackfillResult result = await _sut.BackfillMissedAsync(CancellationToken.None);

        Assert.Equal(0, result.Created);
    }

    [Fact]
    public async Task BackfillMissedAsync_WhenNoUnassignedEvents_ShouldReturnZeroFailed()
    {
        _tollEventRepository.GetUnassignedBeforeDateAsync(Arg.Any<DateOnly>(), Arg.Any<CancellationToken>())
            .Returns([]);

        BackfillResult result = await _sut.BackfillMissedAsync(CancellationToken.None);

        Assert.Equal(0, result.Failed);
    }

    [Fact]
    public async Task BackfillMissedAsync_WhenUnassignedEventsExist_ShouldCallCreateWithTollEventsAsync()
    {
        Car vehicle = CreateVehicle();
        TollEvent tollEvent = new TollEvent(Yesterday.ToDateTime(TimeOnly.MinValue).AddHours(8), "Zone1", vehicle.Id);

        _tollEventRepository.GetUnassignedBeforeDateAsync(Arg.Any<DateOnly>(), Arg.Any<CancellationToken>())
            .Returns([tollEvent]);
        _vehicleRepository.GetVehicleByIdAsync(vehicle.Id, Arg.Any<CancellationToken>())
            .Returns(vehicle);
        _vehicleRepository.GetVehicleByRegistrationNumberAsync("ABC123", Arg.Any<CancellationToken>())
            .Returns(vehicle);
        _dailyTollSummaryRepository.ExistsAsync(vehicle.Id, Yesterday, Arg.Any<CancellationToken>())
            .Returns(false);
        _tollEventRepository.GetAllByRegistrationAsync("ABC123", Yesterday, Arg.Any<CancellationToken>())
            .Returns([tollEvent]);
        _tollCalculator.CalculateDailyTotalFee(Arg.Any<Entities.Bases.Vehicle>(), Arg.Any<DateTimeOffset[]>())
            .Returns(50);
        _dailyTollSummaryRepository.CreateWithTollEventsAsync(
            Arg.Any<DailyTollSummary>(), Arg.Any<IReadOnlyList<TollEvent>>(), Arg.Any<CancellationToken>())
            .Returns(Guid.NewGuid());

        await _sut.BackfillMissedAsync(CancellationToken.None);

        await _dailyTollSummaryRepository.Received(1).CreateWithTollEventsAsync(
            Arg.Any<DailyTollSummary>(), Arg.Any<IReadOnlyList<TollEvent>>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task BackfillMissedAsync_WhenUnassignedEventsExist_ShouldReturnCreatedCount()
    {
        Car vehicle = CreateVehicle();
        TollEvent tollEvent = new TollEvent(Yesterday.ToDateTime(TimeOnly.MinValue).AddHours(8), "Zone1", vehicle.Id);

        _tollEventRepository.GetUnassignedBeforeDateAsync(Arg.Any<DateOnly>(), Arg.Any<CancellationToken>())
            .Returns([tollEvent]);
        _vehicleRepository.GetVehicleByIdAsync(vehicle.Id, Arg.Any<CancellationToken>())
            .Returns(vehicle);
        _vehicleRepository.GetVehicleByRegistrationNumberAsync("ABC123", Arg.Any<CancellationToken>())
            .Returns(vehicle);
        _dailyTollSummaryRepository.ExistsAsync(vehicle.Id, Yesterday, Arg.Any<CancellationToken>())
            .Returns(false);
        _tollEventRepository.GetAllByRegistrationAsync("ABC123", Yesterday, Arg.Any<CancellationToken>())
            .Returns([tollEvent]);
        _tollCalculator.CalculateDailyTotalFee(Arg.Any<Entities.Bases.Vehicle>(), Arg.Any<DateTimeOffset[]>())
            .Returns(50);
        _dailyTollSummaryRepository.CreateWithTollEventsAsync(
            Arg.Any<DailyTollSummary>(), Arg.Any<IReadOnlyList<TollEvent>>(), Arg.Any<CancellationToken>())
            .Returns(Guid.NewGuid());

        BackfillResult result = await _sut.BackfillMissedAsync(CancellationToken.None);

        Assert.Equal(1, result.Created);
    }

    [Fact]
    public async Task BackfillMissedAsync_WhenSummaryAlreadyExists_ShouldReturnSkippedCount()
    {
        Car vehicle = CreateVehicle();
        TollEvent tollEvent = new TollEvent(Yesterday.ToDateTime(TimeOnly.MinValue).AddHours(8), "Zone1", vehicle.Id);

        _tollEventRepository.GetUnassignedBeforeDateAsync(Arg.Any<DateOnly>(), Arg.Any<CancellationToken>())
            .Returns([tollEvent]);
        _vehicleRepository.GetVehicleByIdAsync(vehicle.Id, Arg.Any<CancellationToken>())
            .Returns(vehicle);
        _vehicleRepository.GetVehicleByRegistrationNumberAsync("ABC123", Arg.Any<CancellationToken>())
            .Returns(vehicle);
        _dailyTollSummaryRepository.ExistsAsync(vehicle.Id, Yesterday, Arg.Any<CancellationToken>())
            .Returns(true);

        BackfillResult result = await _sut.BackfillMissedAsync(CancellationToken.None);

        Assert.Equal(1, result.Skipped);
    }

    [Fact]
    public async Task BackfillMissedAsync_WhenOneGroupFailsUnexpectedly_ShouldReturnFailedCount()
    {
        Car vehicle = CreateVehicle();
        TollEvent tollEvent = new TollEvent(Yesterday.ToDateTime(TimeOnly.MinValue).AddHours(8), "Zone1", vehicle.Id);

        _tollEventRepository.GetUnassignedBeforeDateAsync(Arg.Any<DateOnly>(), Arg.Any<CancellationToken>())
            .Returns([tollEvent]);
        _vehicleRepository.GetVehicleByIdAsync(vehicle.Id, Arg.Any<CancellationToken>())
            .Returns(vehicle);
        _vehicleRepository.GetVehicleByRegistrationNumberAsync("ABC123", Arg.Any<CancellationToken>())
            .Returns(vehicle);
        _dailyTollSummaryRepository.ExistsAsync(vehicle.Id, Yesterday, Arg.Any<CancellationToken>())
            .Returns(false);
        _tollEventRepository.GetAllByRegistrationAsync("ABC123", Yesterday, Arg.Any<CancellationToken>())
            .Returns([tollEvent]);
        _tollCalculator.CalculateDailyTotalFee(Arg.Any<Entities.Bases.Vehicle>(), Arg.Any<DateTimeOffset[]>())
            .Returns(50);
        _dailyTollSummaryRepository.CreateWithTollEventsAsync(
            Arg.Any<DailyTollSummary>(), Arg.Any<IReadOnlyList<TollEvent>>(), Arg.Any<CancellationToken>())
            .Throws(new Exception("Unexpected DB error"));

        BackfillResult result = await _sut.BackfillMissedAsync(CancellationToken.None);

        Assert.Equal(1, result.Failed);
    }

    [Fact]
    public async Task BackfillMissedAsync_WhenVehicleNotFoundInRepository_ShouldReturnSkippedCount()
    {
        Car vehicle = CreateVehicle();
        TollEvent tollEvent = new TollEvent(Yesterday.ToDateTime(TimeOnly.MinValue).AddHours(8), "Zone1", vehicle.Id);

        _tollEventRepository.GetUnassignedBeforeDateAsync(Arg.Any<DateOnly>(), Arg.Any<CancellationToken>())
            .Returns([tollEvent]);
        _vehicleRepository.GetVehicleByIdAsync(vehicle.Id, Arg.Any<CancellationToken>())
            .Throws(new InvalidOperationException("Vehicle not found."));

        BackfillResult result = await _sut.BackfillMissedAsync(CancellationToken.None);

        Assert.Equal(1, result.Skipped);
    }
}
