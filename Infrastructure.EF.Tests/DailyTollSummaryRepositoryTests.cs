using EFCore.Repositories;
using Entities.Tolls;
using Entities.Vehicels;
using Infrastructure.EF.Tests.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.EFCore.Tests;

public class DailyTollSummaryRepositoryTests : IDisposable
{
    private readonly SqliteTollDbContextFactory _factory = new();
    private readonly DailyTollSummaryRepository _sut;

    public DailyTollSummaryRepositoryTests()
    {
        _sut = new DailyTollSummaryRepository(_factory);
    }

    private async Task<Guid> SeedVehicleAsync(string registrationNumber)
    {
        await using var arrange = _factory.CreateDbContext();
        var car = new Car(registrationNumber);
        arrange.Add(car);
        await arrange.SaveChangesAsync();
        return car.Id;
    }

    private async Task<TollEvent> SeedTollEventAsync(Guid vehicleId, DateTimeOffset eventDateTime)
    {
        await using var arrange = _factory.CreateDbContext();
        var tollEvent = new TollEvent(eventDateTime, "Centrum", vehicleId);
        arrange.Add(tollEvent);
        await arrange.SaveChangesAsync();
        return tollEvent;
    }

    private async Task<DailyTollSummary> SeedSummaryAsync(Guid vehicleId, DateOnly forDay, decimal amount)
    {
        var summary = new DailyTollSummary(forDay, amount, vehicleId);
        await using var arrange = _factory.CreateDbContext();
        arrange.Add(summary);
        await arrange.SaveChangesAsync();
        return summary;
    }

    private async Task AssignSummaryToInvoiceAsync(DailyTollSummary summary, Guid vehicleId, int year, int month)
    {
        var invoice = new TollInvoice(vehicleId, year, month, []);
        summary.AssignToTollInvoice(invoice.Id);
        await using var db = _factory.CreateDbContext();
        db.Add(invoice);
        db.Entry(summary).State = EntityState.Unchanged;
        db.Entry(summary).Property(s => s.TollInvoiceId).IsModified = true;
        await db.SaveChangesAsync();
    }

    // --- CreateWithTollEventsAsync ---

    [Fact]
    public async Task CreateWithTollEventsAsync_WithValidData_ShouldReturnNonEmptyGuid()
    {
        var vehicleId = await SeedVehicleAsync("ABC123");
        var tollEvent = await SeedTollEventAsync(vehicleId, new DateTimeOffset(2024, 6, 15, 8, 0, 0, TimeSpan.Zero));
        var summary = new DailyTollSummary(new DateOnly(2024, 6, 15), 45m, vehicleId);
        tollEvent.AssignToDailyTollSummary(summary.Id);

        var id = await _sut.CreateWithTollEventsAsync(summary, [tollEvent], CancellationToken.None);

        Assert.NotEqual(Guid.Empty, id);
    }

    [Fact]
    public async Task CreateWithTollEventsAsync_WithValidData_ShouldPersistSummary()
    {
        var vehicleId = await SeedVehicleAsync("ABC123");
        var tollEvent = await SeedTollEventAsync(vehicleId, new DateTimeOffset(2024, 6, 15, 8, 0, 0, TimeSpan.Zero));
        var summary = new DailyTollSummary(new DateOnly(2024, 6, 15), 45m, vehicleId);
        tollEvent.AssignToDailyTollSummary(summary.Id);

        await _sut.CreateWithTollEventsAsync(summary, [tollEvent], CancellationToken.None);

        await using var verify = _factory.CreateDbContext();
        var stored = await verify.DailyTollSummaries.SingleAsync();
        Assert.Equal(summary.Id, stored.Id);
    }

    [Fact]
    public async Task CreateWithTollEventsAsync_WithValidData_ShouldLinkTollEventToSummary()
    {
        var vehicleId = await SeedVehicleAsync("ABC123");
        var tollEvent = await SeedTollEventAsync(vehicleId, new DateTimeOffset(2024, 6, 15, 8, 0, 0, TimeSpan.Zero));
        var summary = new DailyTollSummary(new DateOnly(2024, 6, 15), 45m, vehicleId);
        tollEvent.AssignToDailyTollSummary(summary.Id);

        await _sut.CreateWithTollEventsAsync(summary, [tollEvent], CancellationToken.None);

        await using var verify = _factory.CreateDbContext();
        var storedEvent = await verify.TollEvents.SingleAsync();
        Assert.Equal(summary.Id, storedEvent.DailyTollSummaryId);
    }

    [Fact]
    public async Task CreateWithTollEventsAsync_WhenDuplicateVehicleAndDay_ShouldThrowDbUpdateException()
    {
        var vehicleId = await SeedVehicleAsync("ABC123");
        await SeedSummaryAsync(vehicleId, new DateOnly(2024, 6, 15), 30m);

        var duplicate = new DailyTollSummary(new DateOnly(2024, 6, 15), 20m, vehicleId);

        await Assert.ThrowsAsync<DbUpdateException>(() =>
            _sut.CreateWithTollEventsAsync(duplicate, [], CancellationToken.None));
    }

    [Fact]
    public async Task CreateWithTollEventsAsync_WhenVehicleDoesNotExist_ShouldThrowDbUpdateException()
    {
        var summary = new DailyTollSummary(new DateOnly(2024, 6, 15), 45m, Guid.NewGuid());

        await Assert.ThrowsAsync<DbUpdateException>(() =>
            _sut.CreateWithTollEventsAsync(summary, [], CancellationToken.None));
    }

    // --- ExistsAsync ---

    [Fact]
    public async Task ExistsAsync_WhenSummaryExists_ShouldReturnTrue()
    {
        var vehicleId = await SeedVehicleAsync("ABC123");
        await SeedSummaryAsync(vehicleId, new DateOnly(2024, 6, 15), 30m);

        var result = await _sut.ExistsAsync(vehicleId, new DateOnly(2024, 6, 15), CancellationToken.None);

        Assert.True(result);
    }

    [Fact]
    public async Task ExistsAsync_WhenSummaryDoesNotExist_ShouldReturnFalse()
    {
        var vehicleId = await SeedVehicleAsync("ABC123");

        var result = await _sut.ExistsAsync(vehicleId, new DateOnly(2024, 6, 15), CancellationToken.None);

        Assert.False(result);
    }

    [Fact]
    public async Task ExistsAsync_WhenDifferentVehicle_ShouldReturnFalse()
    {
        var vehicleId = await SeedVehicleAsync("ABC123");
        var otherId = await SeedVehicleAsync("XYZ999");
        await SeedSummaryAsync(vehicleId, new DateOnly(2024, 6, 15), 30m);

        var result = await _sut.ExistsAsync(otherId, new DateOnly(2024, 6, 15), CancellationToken.None);

        Assert.False(result);
    }

    [Fact]
    public async Task ExistsAsync_WhenDifferentDay_ShouldReturnFalse()
    {
        var vehicleId = await SeedVehicleAsync("ABC123");
        await SeedSummaryAsync(vehicleId, new DateOnly(2024, 6, 15), 30m);

        var result = await _sut.ExistsAsync(vehicleId, new DateOnly(2024, 6, 16), CancellationToken.None);

        Assert.False(result);
    }

    // --- GetAllByVehicleIdAsync ---

    [Fact]
    public async Task GetAllByVehicleIdAsync_WhenNoSummariesExist_ShouldReturnEmptyList()
    {
        var vehicleId = await SeedVehicleAsync("ABC123");

        var result = await _sut.GetAllByVehicleIdAsync(vehicleId, CancellationToken.None);

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetAllByVehicleIdAsync_WhenSummariesExist_ShouldReturnOnlyForRequestedVehicle()
    {
        var vehicleId = await SeedVehicleAsync("ABC123");
        var otherId = await SeedVehicleAsync("XYZ999");
        await SeedSummaryAsync(vehicleId, new DateOnly(2024, 6, 15), 30m);
        await SeedSummaryAsync(vehicleId, new DateOnly(2024, 6, 16), 20m);
        await SeedSummaryAsync(otherId, new DateOnly(2024, 6, 15), 50m);

        var result = await _sut.GetAllByVehicleIdAsync(vehicleId, CancellationToken.None);

        Assert.Equal(2, result.Count);
    }

    // --- GetUninvoicedByVehicleIdAsync ---

    [Fact]
    public async Task GetUninvoicedByVehicleIdAsync_WhenNoSummariesExist_ShouldReturnEmptyList()
    {
        var vehicleId = await SeedVehicleAsync("ABC123");

        var result = await _sut.GetUninvoicedByVehicleIdAsync(vehicleId, CancellationToken.None);

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetUninvoicedByVehicleIdAsync_WhenAllAreInvoiced_ShouldReturnEmptyList()
    {
        var vehicleId = await SeedVehicleAsync("ABC123");
        var summary = await SeedSummaryAsync(vehicleId, new DateOnly(2024, 6, 15), 30m);

        await AssignSummaryToInvoiceAsync(summary, vehicleId, 2024, 6);

        var result = await _sut.GetUninvoicedByVehicleIdAsync(vehicleId, CancellationToken.None);

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetUninvoicedByVehicleIdAsync_WhenMixed_ShouldReturnOnlyUninvoiced()
    {
        var vehicleId = await SeedVehicleAsync("ABC123");
        var invoiced = await SeedSummaryAsync(vehicleId, new DateOnly(2024, 6, 15), 30m);
        await SeedSummaryAsync(vehicleId, new DateOnly(2024, 6, 16), 20m);

        await AssignSummaryToInvoiceAsync(invoiced, vehicleId, 2024, 6);

        var result = await _sut.GetUninvoicedByVehicleIdAsync(vehicleId, CancellationToken.None);

        Assert.Single(result);
    }

    [Fact]
    public async Task GetUninvoicedByVehicleIdAsync_ShouldReturnOrderedByForDay()
    {
        var vehicleId = await SeedVehicleAsync("ABC123");
        await SeedSummaryAsync(vehicleId, new DateOnly(2024, 6, 20), 10m);
        await SeedSummaryAsync(vehicleId, new DateOnly(2024, 6, 10), 30m);
        await SeedSummaryAsync(vehicleId, new DateOnly(2024, 6, 15), 20m);

        var result = await _sut.GetUninvoicedByVehicleIdAsync(vehicleId, CancellationToken.None);

        Assert.Equal(new DateOnly(2024, 6, 10), result[0].ForDay);
    }

    // --- GetUninvoicedByVehicleAndMonthAsync ---

    [Fact]
    public async Task GetUninvoicedByVehicleAndMonthAsync_WhenNoSummariesExist_ShouldReturnEmptyList()
    {
        var vehicleId = await SeedVehicleAsync("ABC123");

        var result = await _sut.GetUninvoicedByVehicleAndMonthAsync(vehicleId, 2024, 6, CancellationToken.None);

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetUninvoicedByVehicleAndMonthAsync_ShouldReturnOnlyMatchingMonth()
    {
        var vehicleId = await SeedVehicleAsync("ABC123");
        await SeedSummaryAsync(vehicleId, new DateOnly(2024, 6, 15), 30m);
        await SeedSummaryAsync(vehicleId, new DateOnly(2024, 7, 1), 20m);

        var result = await _sut.GetUninvoicedByVehicleAndMonthAsync(vehicleId, 2024, 6, CancellationToken.None);

        Assert.Single(result);
    }

    [Fact]
    public async Task GetUninvoicedByVehicleAndMonthAsync_ShouldExcludeInvoicedSummaries()
    {
        var vehicleId = await SeedVehicleAsync("ABC123");
        var invoiced = await SeedSummaryAsync(vehicleId, new DateOnly(2024, 6, 15), 30m);
        await SeedSummaryAsync(vehicleId, new DateOnly(2024, 6, 16), 20m);

        await AssignSummaryToInvoiceAsync(invoiced, vehicleId, 2024, 6);

        var result = await _sut.GetUninvoicedByVehicleAndMonthAsync(vehicleId, 2024, 6, CancellationToken.None);

        Assert.Single(result);
    }

    [Fact]
    public async Task GetUninvoicedByVehicleAndMonthAsync_ShouldExcludeOtherVehicle()
    {
        var vehicleId = await SeedVehicleAsync("ABC123");
        var otherId = await SeedVehicleAsync("XYZ999");
        await SeedSummaryAsync(vehicleId, new DateOnly(2024, 6, 15), 30m);
        await SeedSummaryAsync(otherId, new DateOnly(2024, 6, 15), 50m);

        var result = await _sut.GetUninvoicedByVehicleAndMonthAsync(vehicleId, 2024, 6, CancellationToken.None);

        Assert.Single(result);
    }

    [Fact]
    public async Task GetUninvoicedByVehicleAndMonthAsync_ShouldReturnOrderedByForDay()
    {
        var vehicleId = await SeedVehicleAsync("ABC123");
        await SeedSummaryAsync(vehicleId, new DateOnly(2024, 6, 20), 10m);
        await SeedSummaryAsync(vehicleId, new DateOnly(2024, 6, 5), 30m);

        var result = await _sut.GetUninvoicedByVehicleAndMonthAsync(vehicleId, 2024, 6, CancellationToken.None);

        Assert.Equal(new DateOnly(2024, 6, 5), result[0].ForDay);
    }

    public void Dispose() => _factory.Dispose();
}
