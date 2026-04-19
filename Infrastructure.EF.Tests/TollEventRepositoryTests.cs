using EFCore.Repositories;
using Entities.Tolls;
using Entities.Vehicels;
using Infrastructure.EF.Tests.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.EFCore.Tests;

public class TollEventRepositoryTests : IDisposable
{
    private static readonly DateTimeOffset FixedEventDateTime = new(2024, 6, 15, 8, 0, 0, TimeSpan.Zero);

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
            "ABC123", DateOnly.FromDateTime(FixedEventDateTime.UtcDateTime), CancellationToken.None);

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetAllByRegistrationAsync_WhenEventsMatchRegistrationAndDate_ShouldReturnOnlyMatchingEvents()
    {
        var vehicleId = await SeedVehicleAsync("ABC123");
        var otherVehicleId = await SeedVehicleAsync("XYZ999");
        var queryDay = FixedEventDateTime;

        await _sut.CreateTollEventAsync(new TollEvent(queryDay, "Centrum", vehicleId), CancellationToken.None);
        await _sut.CreateTollEventAsync(new TollEvent(queryDay.AddHours(1), "Norr", vehicleId), CancellationToken.None);
        await _sut.CreateTollEventAsync(new TollEvent(queryDay, "Centrum", otherVehicleId), CancellationToken.None);
        await _sut.CreateTollEventAsync(new TollEvent(queryDay.AddDays(-1), "Centrum", vehicleId), CancellationToken.None);

        var result = await _sut.GetAllByRegistrationAsync(
            "ABC123", DateOnly.FromDateTime(queryDay.UtcDateTime), CancellationToken.None);

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task GetAllByRegistrationAsync_WhenRegistrationDoesNotExist_ShouldReturnEmptyList()
    {
        var result = await _sut.GetAllByRegistrationAsync(
            "DOES-NOT-EXIST", DateOnly.FromDateTime(FixedEventDateTime.UtcDateTime), CancellationToken.None);

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetRecentAsync_WhenNoEventsExist_ShouldReturnEmptyList()
    {
        var result = await _sut.GetRecentAsync(5, CancellationToken.None);

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetRecentAsync_WhenEventsExist_ShouldReturnRequestedCount()
    {
        var vehicleId = await SeedVehicleAsync("ABC123");
        await _sut.CreateTollEventAsync(new TollEvent(FixedEventDateTime, "Centrum", vehicleId), CancellationToken.None);
        await _sut.CreateTollEventAsync(new TollEvent(FixedEventDateTime.AddHours(1), "Norr", vehicleId), CancellationToken.None);
        await _sut.CreateTollEventAsync(new TollEvent(FixedEventDateTime.AddHours(2), "Söder", vehicleId), CancellationToken.None);

        var result = await _sut.GetRecentAsync(2, CancellationToken.None);

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task GetRecentAsync_WhenEventsExist_ShouldReturnOrderedByEventDateTimeDescending()
    {
        var vehicleId = await SeedVehicleAsync("ABC123");
        await _sut.CreateTollEventAsync(new TollEvent(FixedEventDateTime, "Centrum", vehicleId), CancellationToken.None);
        await _sut.CreateTollEventAsync(new TollEvent(FixedEventDateTime.AddHours(2), "Söder", vehicleId), CancellationToken.None);
        await _sut.CreateTollEventAsync(new TollEvent(FixedEventDateTime.AddHours(1), "Norr", vehicleId), CancellationToken.None);

        var result = await _sut.GetRecentAsync(3, CancellationToken.None);

        Assert.Equal("Söder", result[0].Zone);
    }

    [Fact]
    public async Task GetRecentAsync_WhenEventsExist_ShouldIncludeVehicle()
    {
        var vehicleId = await SeedVehicleAsync("ABC123");
        await _sut.CreateTollEventAsync(new TollEvent(FixedEventDateTime, "Centrum", vehicleId), CancellationToken.None);

        var result = await _sut.GetRecentAsync(1, CancellationToken.None);

        Assert.NotNull(result[0].Vehicle);
    }

    [Fact]
    public async Task GetRecentAsync_WhenEventsExist_ShouldIncludeVehicleWithCorrectRegistrationNumber()
    {
        var vehicleId = await SeedVehicleAsync("ABC123");
        await _sut.CreateTollEventAsync(new TollEvent(FixedEventDateTime, "Centrum", vehicleId), CancellationToken.None);

        var result = await _sut.GetRecentAsync(1, CancellationToken.None);

        Assert.Equal("ABC123", result[0].Vehicle!.RegistrationNumber);
    }

    [Fact]
    public async Task GetRecentAsync_WhenCountExceedsAvailable_ShouldReturnAllEvents()
    {
        var vehicleId = await SeedVehicleAsync("ABC123");
        await _sut.CreateTollEventAsync(new TollEvent(FixedEventDateTime, "Centrum", vehicleId), CancellationToken.None);
        await _sut.CreateTollEventAsync(new TollEvent(FixedEventDateTime.AddHours(1), "Norr", vehicleId), CancellationToken.None);

        var result = await _sut.GetRecentAsync(10, CancellationToken.None);

        Assert.Equal(2, result.Count);
    }

    // --- GetUnassignedAsync ---

    [Fact]
    public async Task GetUnassignedAsync_WhenNoEventsExist_ShouldReturnEmptyList()
    {
        var result = await _sut.GetUnassignedAsync(10, CancellationToken.None);

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetUnassignedAsync_WhenAllEventsAreAssigned_ShouldReturnEmptyList()
    {
        var vehicleId = await SeedVehicleAsync("ABC123");
        var tollEvent = new TollEvent(FixedEventDateTime, "Centrum", vehicleId);
        await _sut.CreateTollEventAsync(tollEvent, CancellationToken.None);

        // Assign to a daily toll summary
        var summary = new DailyTollSummary(DateOnly.FromDateTime(FixedEventDateTime.UtcDateTime), 30m, vehicleId);
        tollEvent.AssignToDailyTollSummary(summary.Id);
        await using var db = _factory.CreateDbContext();
        db.Add(summary);
        db.TollEvents.Update(tollEvent);
        await db.SaveChangesAsync();

        var result = await _sut.GetUnassignedAsync(10, CancellationToken.None);

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetUnassignedAsync_WhenMixedEvents_ShouldReturnOnlyUnassigned()
    {
        var vehicleId = await SeedVehicleAsync("ABC123");
        var assigned = new TollEvent(FixedEventDateTime, "Centrum", vehicleId);
        await _sut.CreateTollEventAsync(assigned, CancellationToken.None);
        await _sut.CreateTollEventAsync(new TollEvent(FixedEventDateTime.AddHours(1), "Norr", vehicleId), CancellationToken.None);

        var summary = new DailyTollSummary(DateOnly.FromDateTime(FixedEventDateTime.UtcDateTime), 30m, vehicleId);
        assigned.AssignToDailyTollSummary(summary.Id);
        await using var db = _factory.CreateDbContext();
        db.Add(summary);
        db.TollEvents.Update(assigned);
        await db.SaveChangesAsync();

        var result = await _sut.GetUnassignedAsync(10, CancellationToken.None);

        Assert.Single(result);
    }

    [Fact]
    public async Task GetUnassignedAsync_ShouldReturnOrderedByEventDateTimeDescending()
    {
        var vehicleId = await SeedVehicleAsync("ABC123");
        await _sut.CreateTollEventAsync(new TollEvent(FixedEventDateTime, "Centrum", vehicleId), CancellationToken.None);
        await _sut.CreateTollEventAsync(new TollEvent(FixedEventDateTime.AddHours(2), "Söder", vehicleId), CancellationToken.None);
        await _sut.CreateTollEventAsync(new TollEvent(FixedEventDateTime.AddHours(1), "Norr", vehicleId), CancellationToken.None);

        var result = await _sut.GetUnassignedAsync(3, CancellationToken.None);

        Assert.Equal("Söder", result[0].Zone);
    }

    [Fact]
    public async Task GetUnassignedAsync_ShouldIncludeVehicle()
    {
        var vehicleId = await SeedVehicleAsync("ABC123");
        await _sut.CreateTollEventAsync(new TollEvent(FixedEventDateTime, "Centrum", vehicleId), CancellationToken.None);

        var result = await _sut.GetUnassignedAsync(1, CancellationToken.None);

        Assert.NotNull(result[0].Vehicle);
    }

    [Fact]
    public async Task GetUnassignedAsync_ShouldRespectCount()
    {
        var vehicleId = await SeedVehicleAsync("ABC123");
        await _sut.CreateTollEventAsync(new TollEvent(FixedEventDateTime, "Centrum", vehicleId), CancellationToken.None);
        await _sut.CreateTollEventAsync(new TollEvent(FixedEventDateTime.AddHours(1), "Norr", vehicleId), CancellationToken.None);
        await _sut.CreateTollEventAsync(new TollEvent(FixedEventDateTime.AddHours(2), "Söder", vehicleId), CancellationToken.None);

        var result = await _sut.GetUnassignedAsync(2, CancellationToken.None);

        Assert.Equal(2, result.Count);
    }

    public void Dispose() => _factory.Dispose();
}
