using EFCore.Repositories;
using Entities.Tolls;
using Entities.Vehicels;
using Infrastructure.EF.Tests.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.EFCore.Tests;

public class TollEventRepositoryTests : IDisposable
{
    private readonly SqliteTollDbContextFactory _factory = new();
    private readonly TollEventRepository _sut;

    public TollEventRepositoryTests()
    {
        _sut = new TollEventRepository(_factory);
    }

    private async Task<Guid> SeedVehicleAsync(string registrationNumber)
    {
        await using var arrange = _factory.CreateDbContext();
        var car = new Car(registrationNumber);
        arrange.Add(car);
        await arrange.SaveChangesAsync();
        return car.Id;
    }

    [Fact]
    public async Task CreateTollEventAsync_WithValidTollEvent_ShouldReturnNonEmptyGuid()
    {
        var vehicleId = await SeedVehicleAsync("ABC123");
        var tollEvent = new TollEvent(DateTime.UtcNow, "Centrum", vehicleId);

        var id = await _sut.CreateTollEventAsync(tollEvent, CancellationToken.None);

        Assert.NotEqual(Guid.Empty, id);
    }

    [Fact]
    public async Task CreateTollEventAsync_WithValidTollEvent_ShouldPersistTollEventToDatabase()
    {
        var vehicleId = await SeedVehicleAsync("ABC123");
        var tollEvent = new TollEvent(DateTime.UtcNow, "Centrum", vehicleId);

        var id = await _sut.CreateTollEventAsync(tollEvent, CancellationToken.None);

        await using var verify = _factory.CreateDbContext();
        var stored = await verify.TollEvents.SingleAsync();
        Assert.Equal(id, stored.Id);
    }

    [Fact]
    public async Task CreateTollEventAsync_WithValidTollEvent_ShouldPersistZone()
    {
        var vehicleId = await SeedVehicleAsync("ABC123");
        var tollEvent = new TollEvent(DateTime.UtcNow, "Centrum", vehicleId);

        await _sut.CreateTollEventAsync(tollEvent, CancellationToken.None);

        await using var verify = _factory.CreateDbContext();
        var stored = await verify.TollEvents.SingleAsync();
        Assert.Equal("Centrum", stored.Zone);
    }

    [Fact]
    public async Task CreateTollEventAsync_WithValidTollEvent_ShouldPersistVehicleId()
    {
        var vehicleId = await SeedVehicleAsync("ABC123");
        var tollEvent = new TollEvent(DateTime.UtcNow, "Centrum", vehicleId);

        await _sut.CreateTollEventAsync(tollEvent, CancellationToken.None);

        await using var verify = _factory.CreateDbContext();
        var stored = await verify.TollEvents.SingleAsync();
        Assert.Equal(vehicleId, stored.VehicleId);
    }

    [Fact]
    public async Task CreateTollEventAsync_WhenVehicleDoesNotExist_ShouldThrowDbUpdateException()
    {
        var tollEvent = new TollEvent(DateTime.UtcNow, "Centrum", Guid.NewGuid());

        await Assert.ThrowsAsync<DbUpdateException>(() =>
            _sut.CreateTollEventAsync(tollEvent, CancellationToken.None));
    }

    [Fact]
    public async Task GetByIdAsync_WhenTollEventExists_ShouldReturnTollEventWithMatchingId()
    {
        var vehicleId = await SeedVehicleAsync("ABC123");
        var id = await _sut.CreateTollEventAsync(
            new TollEvent(DateTime.UtcNow, "Centrum", vehicleId), CancellationToken.None);

        var result = await _sut.GetByIdAsync(id, CancellationToken.None);

        Assert.Equal(id, result.Id);
    }

    [Fact]
    public async Task GetByIdAsync_WhenIdDoesNotExist_ShouldThrowInvalidOperationException()
    {
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _sut.GetByIdAsync(Guid.NewGuid(), CancellationToken.None));
    }

    [Fact]
    public async Task GetAllByRegistrationAsync_WhenNoEventsExist_ShouldReturnEmptyList()
    {
        await SeedVehicleAsync("ABC123");

        var result = await _sut.GetAllByRegistrationAsync(
            "ABC123", DateOnly.FromDateTime(DateTime.UtcNow), CancellationToken.None);

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetAllByRegistrationAsync_WhenEventsMatchRegistrationAndDate_ShouldReturnOnlyMatchingEvents()
    {
        var vehicleId = await SeedVehicleAsync("ABC123");
        var otherVehicleId = await SeedVehicleAsync("XYZ999");
        var today = DateTime.UtcNow.Date.AddHours(8);

        await _sut.CreateTollEventAsync(new TollEvent(today, "Centrum", vehicleId), CancellationToken.None);
        await _sut.CreateTollEventAsync(new TollEvent(today.AddHours(1), "Norr", vehicleId), CancellationToken.None);
        await _sut.CreateTollEventAsync(new TollEvent(today, "Centrum", otherVehicleId), CancellationToken.None);
        await _sut.CreateTollEventAsync(new TollEvent(today.AddDays(-1), "Centrum", vehicleId), CancellationToken.None);

        var result = await _sut.GetAllByRegistrationAsync(
            "ABC123", DateOnly.FromDateTime(today), CancellationToken.None);

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task GetAllByRegistrationAsync_WhenRegistrationDoesNotExist_ShouldReturnEmptyList()
    {
        var result = await _sut.GetAllByRegistrationAsync(
            "DOES-NOT-EXIST", DateOnly.FromDateTime(DateTime.UtcNow), CancellationToken.None);

        Assert.Empty(result);
    }

    public void Dispose() => _factory.Dispose();
}
