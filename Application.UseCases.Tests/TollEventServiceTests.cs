using Entities.Bases;
using Entities.Interfaces;
using Entities.Tolls;
using Entities.Types;
using Entities.Vehicels;
using NSubstitute;
using UseCases.Dtos;
using UseCases.Interfaces;
using UseCases.Services;

namespace Application.UseCases.Tests;

public class TollEventServiceTests
{
    private readonly IVehicleService _vehicleService = Substitute.For<IVehicleService>();
    private readonly ITollEventRepository _tollEventRepository = Substitute.For<ITollEventRepository>();
    private readonly TollEventService _sut;

    public TollEventServiceTests()
    {
        _sut = new TollEventService(_vehicleService, _tollEventRepository);
    }

    private static VehiclePassageDto Dto() =>
        new("ABC123", DateTime.UtcNow.AddMinutes(-1), "ZoneA", VehicleType.Car);

    private static Vehicle PersistedVehicle() =>
        new Car("ABC123") { Id = Guid.NewGuid() };

    [Fact]
    public async Task RegisterAsync_WithValidDto_ShouldCallGetOrCreateOnVehicleService()
    {
        var vehicle = PersistedVehicle();
        _vehicleService.GetOrCreateAsync(Arg.Any<VehiclePassageDto>(), Arg.Any<CancellationToken>())
            .Returns(vehicle);
        var dto = Dto();

        await _sut.RegisterAsync(dto, CancellationToken.None);

        await _vehicleService.Received(1).GetOrCreateAsync(dto, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RegisterAsync_WithValidDto_ShouldCallCreateTollEventOnRepository()
    {
        var vehicle = PersistedVehicle();
        _vehicleService.GetOrCreateAsync(Arg.Any<VehiclePassageDto>(), Arg.Any<CancellationToken>())
            .Returns(vehicle);

        await _sut.RegisterAsync(Dto(), CancellationToken.None);

        await _tollEventRepository.Received(1).CreateTollEventAsync(
            Arg.Any<TollEvent>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RegisterAsync_WithValidDto_ShouldReturnTollEventBoundToResolvedVehicle()
    {
        var vehicle = PersistedVehicle();
        _vehicleService.GetOrCreateAsync(Arg.Any<VehiclePassageDto>(), Arg.Any<CancellationToken>())
            .Returns(vehicle);

        var result = await _sut.RegisterAsync(Dto(), CancellationToken.None);

        Assert.Equal(vehicle.Id, result.VehicleId);
    }

    [Fact]
    public async Task RegisterAsync_WithValidDto_ShouldReturnTollEventWithDtoEventDateTime()
    {
        var vehicle = PersistedVehicle();
        _vehicleService.GetOrCreateAsync(Arg.Any<VehiclePassageDto>(), Arg.Any<CancellationToken>())
            .Returns(vehicle);
        var dto = Dto();

        var result = await _sut.RegisterAsync(dto, CancellationToken.None);

        Assert.Equal(dto.EventDateTime, result.EventDateTime);
    }

    [Fact]
    public async Task RegisterAsync_WithValidDto_ShouldReturnTollEventWithDtoZone()
    {
        var vehicle = PersistedVehicle();
        _vehicleService.GetOrCreateAsync(Arg.Any<VehiclePassageDto>(), Arg.Any<CancellationToken>())
            .Returns(vehicle);
        var dto = Dto();

        var result = await _sut.RegisterAsync(dto, CancellationToken.None);

        Assert.Equal(dto.Zone, result.Zone);
    }

    [Fact]
    public async Task RegisterAsync_WithNullDto_ShouldThrowArgumentNullException()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            _sut.RegisterAsync(null!, CancellationToken.None));
    }

    [Fact]
    public async Task RegisterAsync_WithNullDto_ShouldNotCallVehicleService()
    {
        try
        {
            await _sut.RegisterAsync(null!, CancellationToken.None);
        }
        catch (ArgumentNullException)
        {
            // expected
        }

        await _vehicleService.DidNotReceive().GetOrCreateAsync(
            Arg.Any<VehiclePassageDto>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RegisterAsync_WithNullDto_ShouldNotCallRepository()
    {
        try
        {
            await _sut.RegisterAsync(null!, CancellationToken.None);
        }
        catch (ArgumentNullException)
        {
            // expected
        }

        await _tollEventRepository.DidNotReceive().CreateTollEventAsync(
            Arg.Any<TollEvent>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetRecentAsync_WithValidCount_ShouldCallGetRecentAsyncOnRepository()
    {
        _tollEventRepository.GetRecentAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(new List<TollEvent>());

        await _sut.GetRecentAsync(10, CancellationToken.None);

        await _tollEventRepository.Received(1).GetRecentAsync(10, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetRecentAsync_WithValidCount_ShouldReturnEventsFromRepository()
    {
        var expected = new List<TollEvent> { new(DateTime.UtcNow.AddMinutes(-1), "ZoneA", Guid.NewGuid()) };
        _tollEventRepository.GetRecentAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(expected);

        var result = await _sut.GetRecentAsync(10, CancellationToken.None);

        Assert.Same(expected, result);
    }

    [Fact]
    public async Task GetRecentAsync_WhenCountIsZero_ShouldThrowArgumentOutOfRangeException()
    {
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            _sut.GetRecentAsync(0, CancellationToken.None));
    }

    [Fact]
    public async Task GetRecentAsync_WhenCountIsNegative_ShouldThrowArgumentOutOfRangeException()
    {
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            _sut.GetRecentAsync(-1, CancellationToken.None));
    }

    [Fact]
    public async Task GetRecentAsync_WhenCountIsInvalid_ShouldNotCallRepository()
    {
        try { await _sut.GetRecentAsync(0, CancellationToken.None); }
        catch (ArgumentOutOfRangeException) { }

        await _tollEventRepository.DidNotReceive().GetRecentAsync(
            Arg.Any<int>(),
            Arg.Any<CancellationToken>());
    }
}
