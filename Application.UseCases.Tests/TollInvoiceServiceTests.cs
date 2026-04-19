using Entities.Bases;
using Entities.Interfaces;
using Entities.Tolls;
using Entities.Vehicels;
using NSubstitute;
using UseCases.Services;

namespace Application.UseCases.Tests;

public class TollInvoiceServiceTests
{
    private readonly IVehicleRepository _vehicleRepository = Substitute.For<IVehicleRepository>();
    private readonly IDailyTollSummaryRepository _dailyTollSummaryRepository = Substitute.For<IDailyTollSummaryRepository>();
    private readonly ITollInvoiceRepository _tollInvoiceRepository = Substitute.For<ITollInvoiceRepository>();
    private readonly TollInvoiceService _sut;

    private const string ValidRegNr = "ABC123";
    private const int ValidYear = 2025;
    private const int ValidMonth = 6;

    public TollInvoiceServiceTests()
    {
        _sut = new TollInvoiceService(_vehicleRepository, _dailyTollSummaryRepository, _tollInvoiceRepository);
    }

    [Fact]
    public async Task CreateAsync_WithNullRegistrationNumber_ShouldThrowArgumentException()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => _sut.CreateAsync(null!, ValidYear, ValidMonth, CancellationToken.None));
    }

    [Fact]
    public async Task CreateAsync_WithEmptyRegistrationNumber_ShouldThrowArgumentException()
    {
        await Assert.ThrowsAsync<ArgumentException>(
            () => _sut.CreateAsync("  ", ValidYear, ValidMonth, CancellationToken.None));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(13)]
    [InlineData(-1)]
    public async Task CreateAsync_WithInvalidMonth_ShouldThrowArgumentOutOfRangeException(int month)
    {
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            () => _sut.CreateAsync(ValidRegNr, ValidYear, month, CancellationToken.None));
    }

    [Fact]
    public async Task CreateAsync_WithInvalidMonth_ShouldThrowWithMonthParamName()
    {
        ArgumentOutOfRangeException ex = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            () => _sut.CreateAsync(ValidRegNr, ValidYear, 0, CancellationToken.None));

        Assert.Equal("month", ex.ParamName);
    }

    [Theory]
    [InlineData(2012)]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task CreateAsync_WithYearBelow2013_ShouldThrowArgumentOutOfRangeException(int year)
    {
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            () => _sut.CreateAsync(ValidRegNr, year, ValidMonth, CancellationToken.None));
    }

    [Fact]
    public async Task CreateAsync_WithYearBelow2013_ShouldThrowWithYearParamName()
    {
        ArgumentOutOfRangeException ex = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            () => _sut.CreateAsync(ValidRegNr, 2012, ValidMonth, CancellationToken.None));

        Assert.Equal("year", ex.ParamName);
    }

    [Fact]
    public async Task CreateAsync_WithFutureYear_ShouldThrowArgumentOutOfRangeException()
    {
        int futureYear = DateTime.UtcNow.Year + 1;

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            () => _sut.CreateAsync(ValidRegNr, futureYear, ValidMonth, CancellationToken.None));
    }

    [Fact]
    public async Task CreateAsync_WithInvalidMonth_ShouldNotCallRepository()
    {
        try { await _sut.CreateAsync(ValidRegNr, ValidYear, 0, CancellationToken.None); } catch { }

        await _vehicleRepository.DidNotReceive()
            .GetVehicleByRegistrationNumberAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateAsync_WhenVehicleNotFound_ShouldThrowInvalidOperationException()
    {
        _vehicleRepository.GetVehicleByRegistrationNumberAsync(ValidRegNr, Arg.Any<CancellationToken>())
            .Returns((Entities.Bases.Vehicle?)null);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _sut.CreateAsync(ValidRegNr, ValidYear, ValidMonth, CancellationToken.None));
    }

    [Fact]
    public async Task CreateAsync_WhenInvoiceAlreadyExists_ShouldThrowInvalidOperationException()
    {
        var vehicle = CreateVehicle();
        _vehicleRepository.GetVehicleByRegistrationNumberAsync(ValidRegNr, Arg.Any<CancellationToken>())
            .Returns(vehicle);
        _tollInvoiceRepository.ExistsAsync(vehicle.Id, ValidYear, ValidMonth, Arg.Any<CancellationToken>())
            .Returns(true);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _sut.CreateAsync(ValidRegNr, ValidYear, ValidMonth, CancellationToken.None));
    }

    [Fact]
    public async Task CreateAsync_WhenNoUninvoicedSummaries_ShouldThrowInvalidOperationException()
    {
        var vehicle = CreateVehicle();
        _vehicleRepository.GetVehicleByRegistrationNumberAsync(ValidRegNr, Arg.Any<CancellationToken>())
            .Returns(vehicle);
        _tollInvoiceRepository.ExistsAsync(vehicle.Id, ValidYear, ValidMonth, Arg.Any<CancellationToken>())
            .Returns(false);
        _dailyTollSummaryRepository.GetUninvoicedByVehicleAndMonthAsync(vehicle.Id, ValidYear, ValidMonth, Arg.Any<CancellationToken>())
            .Returns(new List<DailyTollSummary>());

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _sut.CreateAsync(ValidRegNr, ValidYear, ValidMonth, CancellationToken.None));
    }

    [Fact]
    public async Task CreateAsync_WithValidInput_ShouldCallCreateTollInvoiceAsyncOnRepository()
    {
        var vehicle = CreateVehicle();
        var summaries = new List<DailyTollSummary> { CreateSummary(vehicle.Id) };
        SetupValidScenario(vehicle, summaries);

        await _sut.CreateAsync(ValidRegNr, ValidYear, ValidMonth, CancellationToken.None);

        await _tollInvoiceRepository.Received(1)
            .CreateTollInvoiceAsync(Arg.Any<TollInvoice>(), Arg.Any<List<DailyTollSummary>>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateAsync_WithValidInput_ShouldReturnInvoiceWithCorrectYear()
    {
        var vehicle = CreateVehicle();
        var summaries = new List<DailyTollSummary> { CreateSummary(vehicle.Id) };
        SetupValidScenario(vehicle, summaries);

        TollInvoice result = await _sut.CreateAsync(ValidRegNr, ValidYear, ValidMonth, CancellationToken.None);

        Assert.Equal(ValidYear, result.Year);
    }

    [Fact]
    public async Task CreateAsync_WithValidInput_ShouldReturnInvoiceWithCorrectMonth()
    {
        var vehicle = CreateVehicle();
        var summaries = new List<DailyTollSummary> { CreateSummary(vehicle.Id) };
        SetupValidScenario(vehicle, summaries);

        TollInvoice result = await _sut.CreateAsync(ValidRegNr, ValidYear, ValidMonth, CancellationToken.None);

        Assert.Equal(ValidMonth, result.Month);
    }

    private static Car CreateVehicle() => new(ValidRegNr) { Id = Guid.NewGuid() };

    private void SetupValidScenario(Entities.Bases.Vehicle vehicle, List<DailyTollSummary> summaries)
    {
        _vehicleRepository.GetVehicleByRegistrationNumberAsync(ValidRegNr, Arg.Any<CancellationToken>())
            .Returns(vehicle);
        _tollInvoiceRepository.ExistsAsync(vehicle.Id, ValidYear, ValidMonth, Arg.Any<CancellationToken>())
            .Returns(false);
        _dailyTollSummaryRepository.GetUninvoicedByVehicleAndMonthAsync(vehicle.Id, ValidYear, ValidMonth, Arg.Any<CancellationToken>())
            .Returns(summaries);
    }

    private static DailyTollSummary CreateSummary(Guid vehicleId)
    {
        return Factories.DailyTollSummaryFactory.Create(new DateOnly(ValidYear, ValidMonth, 15), 18, vehicleId);
    }
}
