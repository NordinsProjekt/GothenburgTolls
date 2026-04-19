using Entities.Bases;
using Entities.Interfaces;
using Entities.Types;
using Entities.Vehicels;
using NSubstitute;
using UseCases.Dtos;
using UseCases.Services;

namespace Application.UseCases.Tests;

public class VehicleServiceTests
{
    private readonly IVehicleRepository _repository = Substitute.For<IVehicleRepository>();
    private readonly VehicleService _sut;

    public VehicleServiceTests()
    {
        _sut = new VehicleService(_repository);
    }

    [Fact]
    public async Task GetOrCreateAsync_WhenVehicleAlreadyInRepository_ShouldReturnExistingVehicle()
    {
        var existing = new Car("ABC123");
        _repository
            .GetVehicleByRegistrationNumberAsync("ABC123", Arg.Any<CancellationToken>())
            .Returns(existing);
        var dto = new VehiclePassageDto("ABC123", DateTime.UtcNow, "ZoneA", VehicleType.Car);

        var result = await _sut.GetOrCreateAsync(dto, CancellationToken.None);

        Assert.Same(existing, result);
    }

    [Fact]
    public async Task GetOrCreateAsync_WhenVehicleAlreadyInRepository_ShouldNotCallCreateVehicle()
    {
        var existing = new Car("ABC123");
        _repository
            .GetVehicleByRegistrationNumberAsync("ABC123", Arg.Any<CancellationToken>())
            .Returns(existing);
        var dto = new VehiclePassageDto("ABC123", DateTime.UtcNow, "ZoneA", VehicleType.Car);

        await _sut.GetOrCreateAsync(dto, CancellationToken.None);

        await _repository.DidNotReceive().CreateVehicle(Arg.Any<Vehicle>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetOrCreateAsync_WhenVehicleMissing_ShouldReturnVehicleOfDtoType()
    {
        _repository
            .GetVehicleByRegistrationNumberAsync("XYZ789", Arg.Any<CancellationToken>())
            .Returns((Vehicle?)null);
        var dto = new VehiclePassageDto("XYZ789", DateTime.UtcNow, "ZoneB", VehicleType.Motorbike);

        var result = await _sut.GetOrCreateAsync(dto, CancellationToken.None);

        Assert.IsType<Motorbike>(result);
    }

    [Fact]
    public async Task GetOrCreateAsync_WhenVehicleMissing_ShouldReturnVehicleWithDtoRegistrationNumber()
    {
        _repository
            .GetVehicleByRegistrationNumberAsync("XYZ789", Arg.Any<CancellationToken>())
            .Returns((Vehicle?)null);
        var dto = new VehiclePassageDto("XYZ789", DateTime.UtcNow, "ZoneB", VehicleType.Motorbike);

        var result = await _sut.GetOrCreateAsync(dto, CancellationToken.None);

        Assert.Equal("XYZ789", result.RegistrationNumber);
    }

    [Fact]
    public async Task GetOrCreateAsync_WhenVehicleMissing_ShouldCallCreateVehicleOnRepository()
    {
        _repository
            .GetVehicleByRegistrationNumberAsync("XYZ789", Arg.Any<CancellationToken>())
            .Returns((Vehicle?)null);
        var dto = new VehiclePassageDto("XYZ789", DateTime.UtcNow, "ZoneB", VehicleType.Motorbike);

        await _sut.GetOrCreateAsync(dto, CancellationToken.None);

        await _repository.Received(1).CreateVehicle(
            Arg.Is<Vehicle>(v => v is Motorbike && v.RegistrationNumber == "XYZ789"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetOrCreateAsync_WithNullDto_ShouldThrowArgumentNullException()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            _sut.GetOrCreateAsync(null!, CancellationToken.None));
    }
}
