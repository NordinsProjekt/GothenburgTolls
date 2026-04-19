using Entities.Interfaces;
using Entities.Tolls;
using Entities.Vehicels;
using NSubstitute;
using UseCases.HelperClass;
using UseCases.Interfaces;
using UseCases.Services;

namespace Application.UseCases.Tests;

public class DailyTollSummaryServiceTests
{
    private readonly IVehicleRepository _vehicleRepository = Substitute.For<IVehicleRepository>();
    private readonly ITollEventRepository _tollEventRepository = Substitute.For<ITollEventRepository>();
    private readonly IDailyTollSummaryRepository _dailyTollSummaryRepository = Substitute.For<IDailyTollSummaryRepository>();
    private readonly TollCalculator _tollCalculator;
    private readonly DailyTollSummaryService _sut;

    private static readonly DateOnly ValidDay = new(2025, 6, 16); // Monday
    private const string ValidRegNr = "ABC123";

    public DailyTollSummaryServiceTests()
    {
        var holidayService = Substitute.For<ISwedishHolidayService>();
        holidayService.IsPublicHoliday(Arg.Any<DateOnly>()).Returns(false);
        holidayService.IsDayBeforePublicHoliday(Arg.Any<DateOnly>()).Returns(false);

        var tollRateService = Substitute.For<ITollRateService>();
        tollRateService.MaxDailyFee.Returns(60);
        tollRateService.GetFeeForTime(Arg.Any<TimeOnly>()).Returns(18);

        _tollCalculator = new TollCalculator(holidayService, tollRateService);

        _sut = new DailyTollSummaryService(
            _vehicleRepository,
            _tollEventRepository,
            _dailyTollSummaryRepository,
            _tollCalculator);
    }

    private Car SetupVehicle()
    {
        var vehicle = new Car(ValidRegNr) { Id = Guid.NewGuid() };
        _vehicleRepository.GetVehicleByRegistrationNumberAsync(ValidRegNr, Arg.Any<CancellationToken>())
            .Returns(vehicle);
        return vehicle;
    }

    private void SetupTollEvents(Guid vehicleId)
    {
        var events = new List<TollEvent>
        {
            new(new DateTimeOffset(2025, 6, 16, 7, 30, 0, TimeSpan.FromHours(2)), "ZoneA", vehicleId)
        };
        _tollEventRepository.GetAllByRegistrationAsync(ValidRegNr, ValidDay, Arg.Any<CancellationToken>())
            .Returns(events);
    }

    [Fact]
    public async Task CreateAsync_WithTodaysDate_ShouldThrowArgumentOutOfRangeException()
    {
        var today = DateOnly.FromDateTime(DateTime.Today);

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            _sut.CreateAsync(ValidRegNr, today, CancellationToken.None));
    }

    [Fact]
    public async Task CreateAsync_WithFutureDate_ShouldThrowArgumentOutOfRangeException()
    {
        var futureDate = DateOnly.FromDateTime(DateTime.Today.AddDays(30));

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            _sut.CreateAsync(ValidRegNr, futureDate, CancellationToken.None));
    }

    [Fact]
    public async Task CreateAsync_WithFutureDate_ShouldThrowWithForDayParamName()
    {
        var futureDate = DateOnly.FromDateTime(DateTime.Today.AddDays(1));

        var ex = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            _sut.CreateAsync(ValidRegNr, futureDate, CancellationToken.None));

        Assert.Equal("forDay", ex.ParamName);
    }

    [Fact]
    public async Task CreateAsync_WhenSummaryAlreadyExists_ShouldThrowInvalidOperationException()
    {
        var vehicle = SetupVehicle();
        _dailyTollSummaryRepository.ExistsAsync(vehicle.Id, ValidDay, Arg.Any<CancellationToken>())
            .Returns(true);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _sut.CreateAsync(ValidRegNr, ValidDay, CancellationToken.None));
    }

    [Fact]
    public async Task CreateAsync_WithValidInput_ShouldCallGetVehicleByRegistrationNumber()
    {
        var vehicle = SetupVehicle();
        SetupTollEvents(vehicle.Id);

        await _sut.CreateAsync(ValidRegNr, ValidDay, CancellationToken.None);

        await _vehicleRepository.Received(1).GetVehicleByRegistrationNumberAsync(ValidRegNr, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateAsync_WithValidInput_ShouldCallGetAllByRegistrationAsync()
    {
        var vehicle = SetupVehicle();
        SetupTollEvents(vehicle.Id);

        await _sut.CreateAsync(ValidRegNr, ValidDay, CancellationToken.None);

        await _tollEventRepository.Received(1).GetAllByRegistrationAsync(ValidRegNr, ValidDay, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateAsync_WithValidInput_ShouldCallCreateDailyTollSummaryOnRepository()
    {
        var vehicle = SetupVehicle();
        SetupTollEvents(vehicle.Id);

        await _sut.CreateAsync(ValidRegNr, ValidDay, CancellationToken.None);

        await _dailyTollSummaryRepository.Received(1).CreateDailyTollSummaryAsync(
            Arg.Any<DailyTollSummary>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateAsync_WithValidInput_ShouldReturnSummaryWithCorrectForDay()
    {
        var vehicle = SetupVehicle();
        SetupTollEvents(vehicle.Id);

        DailyTollSummary result = await _sut.CreateAsync(ValidRegNr, ValidDay, CancellationToken.None);

        Assert.Equal(ValidDay, result.ForDay);
    }

    [Fact]
    public async Task CreateAsync_WithValidInput_ShouldReturnSummaryWithCorrectVehicleId()
    {
        var vehicle = SetupVehicle();
        SetupTollEvents(vehicle.Id);

        DailyTollSummary result = await _sut.CreateAsync(ValidRegNr, ValidDay, CancellationToken.None);

        Assert.Equal(vehicle.Id, result.VehicleId);
    }

    [Fact]
    public async Task CreateAsync_WithValidInput_ShouldReturnSummaryWithCalculatedAmount()
    {
        var vehicle = SetupVehicle();
        SetupTollEvents(vehicle.Id);

        DailyTollSummary result = await _sut.CreateAsync(ValidRegNr, ValidDay, CancellationToken.None);

        Assert.Equal(18m, result.Amount);
    }

    [Fact]
    public async Task CreateAsync_WhenVehicleNotFound_ShouldThrowInvalidOperationException()
    {
        _vehicleRepository.GetVehicleByRegistrationNumberAsync(ValidRegNr, Arg.Any<CancellationToken>())
            .Returns((Entities.Bases.Vehicle?)null);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _sut.CreateAsync(ValidRegNr, ValidDay, CancellationToken.None));
    }

    [Fact]
    public async Task CreateAsync_WithNullRegistrationNumber_ShouldThrowArgumentException()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            _sut.CreateAsync(null!, ValidDay, CancellationToken.None));
    }

    [Fact]
    public async Task CreateAsync_WithEmptyRegistrationNumber_ShouldThrowArgumentException()
    {
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _sut.CreateAsync("", ValidDay, CancellationToken.None));
    }

    [Fact]
    public async Task CreateAsync_WithValidInput_ShouldCallUpdateTollEventsOnRepository()
    {
        var vehicle = SetupVehicle();
        SetupTollEvents(vehicle.Id);

        await _sut.CreateAsync(ValidRegNr, ValidDay, CancellationToken.None);

        await _tollEventRepository.Received(1).UpdateTollEventsAsync(
            Arg.Any<IReadOnlyList<TollEvent>>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateAsync_WithValidInput_ShouldAssignSummaryIdToTollEvents()
    {
        var vehicle = SetupVehicle();
        var events = new List<TollEvent>
        {
            new(new DateTimeOffset(2025, 6, 16, 7, 30, 0, TimeSpan.FromHours(2)), "ZoneA", vehicle.Id)
        };
        _tollEventRepository.GetAllByRegistrationAsync(ValidRegNr, ValidDay, Arg.Any<CancellationToken>())
            .Returns(events);

        DailyTollSummary result = await _sut.CreateAsync(ValidRegNr, ValidDay, CancellationToken.None);

        Assert.Equal(result.Id, events[0].DailyTollSummaryId);
    }

    [Fact]
    public async Task CreateAsync_WhenNoTollEventsFound_ShouldThrowInvalidOperationException()
    {
        var vehicle = SetupVehicle();
        _tollEventRepository.GetAllByRegistrationAsync(ValidRegNr, ValidDay, Arg.Any<CancellationToken>())
            .Returns(new List<TollEvent>());

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _sut.CreateAsync(ValidRegNr, ValidDay, CancellationToken.None));
    }

    [Fact]
    public async Task GetAllByVehicleIdAsync_WithValidId_ShouldCallRepository()
    {
        var vehicleId = Guid.NewGuid();
        _dailyTollSummaryRepository.GetAllByVehicleIdAsync(vehicleId, Arg.Any<CancellationToken>())
            .Returns(new List<DailyTollSummary>());

        await _sut.GetAllByVehicleIdAsync(vehicleId, CancellationToken.None);

        await _dailyTollSummaryRepository.Received(1).GetAllByVehicleIdAsync(vehicleId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetAllByVehicleIdAsync_WithEmptyGuid_ShouldThrowArgumentOutOfRangeException()
    {
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            _sut.GetAllByVehicleIdAsync(Guid.Empty, CancellationToken.None));
    }
}
