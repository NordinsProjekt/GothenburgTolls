using EFCore.Repositories;
using Entities.Tolls;
using Entities.Vehicels;
using Infrastructure.EF.Tests.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.EFCore.Tests;

public class TollInvoiceRepositoryTests : IDisposable
{
    private readonly SqliteTollDbContextFactory _factory = new();
    private readonly TollInvoiceRepository _sut;

    public TollInvoiceRepositoryTests()
    {
        _sut = new TollInvoiceRepository(_factory);
    }

    private async Task<Guid> SeedVehicleAsync(string registrationNumber)
    {
        await using var arrange = _factory.CreateDbContext();
        var car = new Car(registrationNumber);
        arrange.Add(car);
        await arrange.SaveChangesAsync();
        return car.Id;
    }

    private async Task<DailyTollSummary> SeedSummaryAsync(Guid vehicleId, DateOnly forDay, decimal amount)
    {
        var summary = new DailyTollSummary(forDay, amount, vehicleId);
        await using var arrange = _factory.CreateDbContext();
        arrange.Add(summary);
        await arrange.SaveChangesAsync();
        return summary;
    }

    // --- CreateTollInvoiceAsync ---

    [Fact]
    public async Task CreateTollInvoiceAsync_WithValidData_ShouldReturnNonEmptyGuid()
    {
        var vehicleId = await SeedVehicleAsync("ABC123");
        var summary = await SeedSummaryAsync(vehicleId, new DateOnly(2024, 6, 15), 30m);
        var invoice = new TollInvoice(vehicleId, 2024, 6, []);
        summary.AssignToTollInvoice(invoice.Id);

        var id = await _sut.CreateTollInvoiceAsync(invoice, [summary], CancellationToken.None);

        Assert.NotEqual(Guid.Empty, id);
    }

    [Fact]
    public async Task CreateTollInvoiceAsync_WithValidData_ShouldPersistInvoice()
    {
        var vehicleId = await SeedVehicleAsync("ABC123");
        var summary = await SeedSummaryAsync(vehicleId, new DateOnly(2024, 6, 15), 30m);
        var invoice = new TollInvoice(vehicleId, 2024, 6, []);
        summary.AssignToTollInvoice(invoice.Id);

        var id = await _sut.CreateTollInvoiceAsync(invoice, [summary], CancellationToken.None);

        await using var verify = _factory.CreateDbContext();
        var stored = await verify.TollInvoices.SingleAsync();
        Assert.Equal(id, stored.Id);
    }

    [Fact]
    public async Task CreateTollInvoiceAsync_WithValidData_ShouldPersistYearAndMonth()
    {
        var vehicleId = await SeedVehicleAsync("ABC123");
        var summary = await SeedSummaryAsync(vehicleId, new DateOnly(2024, 6, 15), 30m);
        var invoice = new TollInvoice(vehicleId, 2024, 6, []);
        summary.AssignToTollInvoice(invoice.Id);

        await _sut.CreateTollInvoiceAsync(invoice, [summary], CancellationToken.None);

        await using var verify = _factory.CreateDbContext();
        var stored = await verify.TollInvoices.SingleAsync();
        Assert.Equal(2024, stored.Year);
    }

    [Fact]
    public async Task CreateTollInvoiceAsync_WithValidData_ShouldPersistMonth()
    {
        var vehicleId = await SeedVehicleAsync("ABC123");
        var summary = await SeedSummaryAsync(vehicleId, new DateOnly(2024, 6, 15), 30m);
        var invoice = new TollInvoice(vehicleId, 2024, 6, []);
        summary.AssignToTollInvoice(invoice.Id);

        await _sut.CreateTollInvoiceAsync(invoice, [summary], CancellationToken.None);

        await using var verify = _factory.CreateDbContext();
        var stored = await verify.TollInvoices.SingleAsync();
        Assert.Equal(6, stored.Month);
    }

    [Fact]
    public async Task CreateTollInvoiceAsync_WithValidData_ShouldLinkSummaryToInvoice()
    {
        var vehicleId = await SeedVehicleAsync("ABC123");
        var summary = await SeedSummaryAsync(vehicleId, new DateOnly(2024, 6, 15), 30m);
        var invoice = new TollInvoice(vehicleId, 2024, 6, []);
        summary.AssignToTollInvoice(invoice.Id);

        await _sut.CreateTollInvoiceAsync(invoice, [summary], CancellationToken.None);

        await using var verify = _factory.CreateDbContext();
        var storedSummary = await verify.DailyTollSummaries.SingleAsync();
        Assert.Equal(invoice.Id, storedSummary.TollInvoiceId);
    }

    [Fact]
    public async Task CreateTollInvoiceAsync_WhenDuplicateVehicleYearMonth_ShouldThrowDbUpdateException()
    {
        var vehicleId = await SeedVehicleAsync("ABC123");
        var s1 = await SeedSummaryAsync(vehicleId, new DateOnly(2024, 6, 15), 30m);
        var invoice1 = new TollInvoice(vehicleId, 2024, 6, []);
        s1.AssignToTollInvoice(invoice1.Id);
        await _sut.CreateTollInvoiceAsync(invoice1, [s1], CancellationToken.None);

        var s2 = await SeedSummaryAsync(vehicleId, new DateOnly(2024, 6, 16), 20m);
        var invoice2 = new TollInvoice(vehicleId, 2024, 6, []);
        s2.AssignToTollInvoice(invoice2.Id);

        await Assert.ThrowsAsync<DbUpdateException>(() =>
            _sut.CreateTollInvoiceAsync(invoice2, [s2], CancellationToken.None));
    }

    [Fact]
    public async Task CreateTollInvoiceAsync_WhenVehicleDoesNotExist_ShouldThrowDbUpdateException()
    {
        var invoice = new TollInvoice(Guid.NewGuid(), 2024, 6, []);

        await Assert.ThrowsAsync<DbUpdateException>(() =>
            _sut.CreateTollInvoiceAsync(invoice, [], CancellationToken.None));
    }

    [Fact]
    public async Task CreateTollInvoiceAsync_WhenDifferentMonth_ShouldAllowCreation()
    {
        var vehicleId = await SeedVehicleAsync("ABC123");
        var s1 = await SeedSummaryAsync(vehicleId, new DateOnly(2024, 6, 15), 30m);
        var invoice1 = new TollInvoice(vehicleId, 2024, 6, []);
        s1.AssignToTollInvoice(invoice1.Id);
        await _sut.CreateTollInvoiceAsync(invoice1, [s1], CancellationToken.None);

        var s2 = await SeedSummaryAsync(vehicleId, new DateOnly(2024, 7, 1), 20m);
        var invoice2 = new TollInvoice(vehicleId, 2024, 7, []);
        s2.AssignToTollInvoice(invoice2.Id);

        var id = await _sut.CreateTollInvoiceAsync(invoice2, [s2], CancellationToken.None);

        Assert.NotEqual(Guid.Empty, id);
    }

    // --- ExistsAsync ---

    [Fact]
    public async Task ExistsAsync_WhenInvoiceExists_ShouldReturnTrue()
    {
        var vehicleId = await SeedVehicleAsync("ABC123");
        var s = await SeedSummaryAsync(vehicleId, new DateOnly(2024, 6, 15), 30m);
        var invoice = new TollInvoice(vehicleId, 2024, 6, []);
        s.AssignToTollInvoice(invoice.Id);
        await _sut.CreateTollInvoiceAsync(invoice, [s], CancellationToken.None);

        var result = await _sut.ExistsAsync(vehicleId, 2024, 6, CancellationToken.None);

        Assert.True(result);
    }

    [Fact]
    public async Task ExistsAsync_WhenNoInvoiceExists_ShouldReturnFalse()
    {
        var vehicleId = await SeedVehicleAsync("ABC123");

        var result = await _sut.ExistsAsync(vehicleId, 2024, 6, CancellationToken.None);

        Assert.False(result);
    }

    [Fact]
    public async Task ExistsAsync_WhenDifferentMonth_ShouldReturnFalse()
    {
        var vehicleId = await SeedVehicleAsync("ABC123");
        var s = await SeedSummaryAsync(vehicleId, new DateOnly(2024, 6, 15), 30m);
        var invoice = new TollInvoice(vehicleId, 2024, 6, []);
        s.AssignToTollInvoice(invoice.Id);
        await _sut.CreateTollInvoiceAsync(invoice, [s], CancellationToken.None);

        var result = await _sut.ExistsAsync(vehicleId, 2024, 7, CancellationToken.None);

        Assert.False(result);
    }

    [Fact]
    public async Task ExistsAsync_WhenDifferentVehicle_ShouldReturnFalse()
    {
        var vehicleId = await SeedVehicleAsync("ABC123");
        var otherId = await SeedVehicleAsync("XYZ999");
        var s = await SeedSummaryAsync(vehicleId, new DateOnly(2024, 6, 15), 30m);
        var invoice = new TollInvoice(vehicleId, 2024, 6, []);
        s.AssignToTollInvoice(invoice.Id);
        await _sut.CreateTollInvoiceAsync(invoice, [s], CancellationToken.None);

        var result = await _sut.ExistsAsync(otherId, 2024, 6, CancellationToken.None);

        Assert.False(result);
    }

    // --- GetAllAsync ---

    [Fact]
    public async Task GetAllAsync_WhenNoInvoicesExist_ShouldReturnEmptyList()
    {
        var result = await _sut.GetAllAsync(CancellationToken.None);

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetAllAsync_WhenInvoicesExist_ShouldReturnAll()
    {
        var v1 = await SeedVehicleAsync("ABC123");
        var v2 = await SeedVehicleAsync("XYZ999");
        var s1 = await SeedSummaryAsync(v1, new DateOnly(2024, 6, 15), 30m);
        var s2 = await SeedSummaryAsync(v2, new DateOnly(2024, 6, 15), 50m);
        var inv1 = new TollInvoice(v1, 2024, 6, []);
        var inv2 = new TollInvoice(v2, 2024, 6, []);
        s1.AssignToTollInvoice(inv1.Id);
        s2.AssignToTollInvoice(inv2.Id);
        await _sut.CreateTollInvoiceAsync(inv1, [s1], CancellationToken.None);
        await _sut.CreateTollInvoiceAsync(inv2, [s2], CancellationToken.None);

        var result = await _sut.GetAllAsync(CancellationToken.None);

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task GetAllAsync_ShouldIncludeVehicle()
    {
        var vehicleId = await SeedVehicleAsync("ABC123");
        var s = await SeedSummaryAsync(vehicleId, new DateOnly(2024, 6, 15), 30m);
        var invoice = new TollInvoice(vehicleId, 2024, 6, []);
        s.AssignToTollInvoice(invoice.Id);
        await _sut.CreateTollInvoiceAsync(invoice, [s], CancellationToken.None);

        var result = await _sut.GetAllAsync(CancellationToken.None);

        Assert.NotNull(result[0].Vehicle);
    }

    [Fact]
    public async Task GetAllAsync_ShouldIncludeVehicleRegistrationNumber()
    {
        var vehicleId = await SeedVehicleAsync("ABC123");
        var s = await SeedSummaryAsync(vehicleId, new DateOnly(2024, 6, 15), 30m);
        var invoice = new TollInvoice(vehicleId, 2024, 6, []);
        s.AssignToTollInvoice(invoice.Id);
        await _sut.CreateTollInvoiceAsync(invoice, [s], CancellationToken.None);

        var result = await _sut.GetAllAsync(CancellationToken.None);

        Assert.Equal("ABC123", result[0].Vehicle!.RegistrationNumber);
    }

    [Fact]
    public async Task GetAllAsync_ShouldIncludeTollSummary()
    {
        var vehicleId = await SeedVehicleAsync("ABC123");
        var s = await SeedSummaryAsync(vehicleId, new DateOnly(2024, 6, 15), 30m);
        var invoice = new TollInvoice(vehicleId, 2024, 6, []);
        s.AssignToTollInvoice(invoice.Id);
        await _sut.CreateTollInvoiceAsync(invoice, [s], CancellationToken.None);

        var result = await _sut.GetAllAsync(CancellationToken.None);

        Assert.Single(result[0].TollSummary);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnOrderedByCreatedDescending()
    {
        var v1 = await SeedVehicleAsync("AAA111");
        var v2 = await SeedVehicleAsync("BBB222");
        var s1 = await SeedSummaryAsync(v1, new DateOnly(2024, 5, 15), 30m);
        var s2 = await SeedSummaryAsync(v2, new DateOnly(2024, 6, 15), 50m);
        var inv1 = new TollInvoice(v1, 2024, 5, []);
        s1.AssignToTollInvoice(inv1.Id);
        await _sut.CreateTollInvoiceAsync(inv1, [s1], CancellationToken.None);

        var inv2 = new TollInvoice(v2, 2024, 6, []);
        s2.AssignToTollInvoice(inv2.Id);
        await _sut.CreateTollInvoiceAsync(inv2, [s2], CancellationToken.None);

        var result = await _sut.GetAllAsync(CancellationToken.None);

        Assert.Equal(inv2.Id, result[0].Id);
    }

    public void Dispose() => _factory.Dispose();
}
