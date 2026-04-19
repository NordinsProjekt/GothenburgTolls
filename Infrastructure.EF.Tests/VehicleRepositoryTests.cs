using EFCore.Repositories;
using Entities.Vehicels;
using Infrastructure.EF.Tests.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.EFCore.Tests;

public class VehicleRepositoryTests : IDisposable
{
    private readonly SqliteTollDbContextFactory _factory = new();
    private readonly VehicleRepository _sut;

    public VehicleRepositoryTests()
    {
        _sut = new VehicleRepository(_factory);
    }

    [Fact]
    public async Task CreateVehicle_WithValidVehicle_ShouldReturnNonEmptyGuid()
    {
        var car = new Car("ABC123");

        var id = await _sut.CreateVehicle(car, CancellationToken.None);

        Assert.NotEqual(Guid.Empty, id);
    }

    [Fact]
    public async Task CreateVehicle_WithValidVehicle_ShouldPersistVehicleToDatabase()
    {
        var car = new Car("ABC123");

        var id = await _sut.CreateVehicle(car, CancellationToken.None);

        await using var verify = _factory.CreateDbContext();
        var stored = await verify.Vehicles.SingleAsync();
        Assert.Equal(id, stored.Id);
        Assert.Equal("ABC123", stored.RegistrationNumber);
        Assert.IsType<Car>(stored);
    }

    [Fact]
    public async Task CreateVehicle_WhenRegistrationNumberAlreadyExists_ShouldThrowDbUpdateException()
    {
        await _sut.CreateVehicle(new Car("DUPE-1"), CancellationToken.None);

        var ex = await Assert.ThrowsAsync<DbUpdateException>(() =>
            _sut.CreateVehicle(new Motorbike("DUPE-1"), CancellationToken.None));

        Assert.NotNull(ex.InnerException);
    }

    [Fact]
    public async Task GetVehicleByIdAsync_WhenVehicleExists_ShouldReturnVehicleWithMatchingId()
    {
        var diplomat = new Diplomat("DIP-1");
        var id = await _sut.CreateVehicle(diplomat, CancellationToken.None);

        var result = await _sut.GetVehicleByIdAsync(id, CancellationToken.None);

        Assert.Equal(id, result.Id);
        Assert.IsType<Diplomat>(result);
        Assert.Equal("DIP-1", result.RegistrationNumber);
    }

    [Fact]
    public async Task GetVehicleByIdAsync_WhenIdDoesNotExist_ShouldThrowInvalidOperationException()
    {
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _sut.GetVehicleByIdAsync(Guid.NewGuid(), CancellationToken.None));
    }

    [Fact]
    public async Task GetVehicleByRegistrationNumberAsync_WhenVehicleExists_ShouldReturnVehicle()
    {
        await using (var arrange = _factory.CreateDbContext())
        {
            arrange.Add(new Motorbike("MC-99"));
            await arrange.SaveChangesAsync();
        }

        var result = await _sut.GetVehicleByRegistrationNumberAsync("MC-99", CancellationToken.None);

        Assert.NotNull(result);
        Assert.IsType<Motorbike>(result);
        Assert.Equal("MC-99", result!.RegistrationNumber);
    }

    [Fact]
    public async Task GetVehicleByRegistrationNumberAsync_WhenVehicleDoesNotExist_ShouldReturnNull()
    {
        var result = await _sut.GetVehicleByRegistrationNumberAsync("DOES-NOT-EXIST", CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetAllVehicleAsync_WhenDatabaseIsEmpty_ShouldReturnEmptyList()
    {
        var all = await _sut.GetAllVehicleAsync(CancellationToken.None);

        Assert.Empty(all);
    }

    [Fact]
    public async Task GetAllVehicleAsync_WhenMultipleVehicleSubclassesPersisted_ShouldReturnAllAcrossSubclasses()
    {
        await using (var arrange = _factory.CreateDbContext())
        {
            arrange.AddRange(new Car("AAA111"), new Motorbike("BBB222"), new Tractor("CCC333"));
            await arrange.SaveChangesAsync();
        }

        var all = await _sut.GetAllVehicleAsync(CancellationToken.None);

        Assert.Equal(3, all.Count);
        Assert.Contains(all, v => v is Car && v.RegistrationNumber == "AAA111");
        Assert.Contains(all, v => v is Motorbike && v.RegistrationNumber == "BBB222");
        Assert.Contains(all, v => v is Tractor && v.RegistrationNumber == "CCC333");
    }

    [Fact]
    public async Task GetVehicleByTypeAsync_WithMixedSubclasses_ShouldReturnOnlyRequestedSubclass()
    {
        await using (var arrange = _factory.CreateDbContext())
        {
            arrange.AddRange(new Car("CAR-1"), new Car("CAR-2"), new Motorbike("MC-1"));
            await arrange.SaveChangesAsync();
        }

        var cars = await _sut.GetVehicleByTypeAsync<Car>(CancellationToken.None);

        Assert.Equal(2, cars.Count);
        Assert.All(cars, c => Assert.IsType<Car>(c));
    }

    [Fact]
    public async Task GetVehicleByTypeAsync_WhenNoMatchingSubclassExists_ShouldReturnEmptyList()
    {
        await using (var arrange = _factory.CreateDbContext())
        {
            arrange.AddRange(new Car("CAR-1"), new Motorbike("MC-1"));
            await arrange.SaveChangesAsync();
        }

        var tractors = await _sut.GetVehicleByTypeAsync<Tractor>(CancellationToken.None);

        Assert.Empty(tractors);
    }

    public void Dispose() => _factory.Dispose();
}
