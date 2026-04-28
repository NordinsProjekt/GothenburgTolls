using Entities.Bases;
using Entities.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace EFCore.Repositories;

public class VehicleRepository(IDbContextFactory<TollDbContext> contextFactory) : IVehicleRepository
{
    public async Task<Guid> CreateVehicle(Vehicle vehicle, CancellationToken cancellationToken)
    {
        await using var db = await contextFactory.CreateDbContextAsync(cancellationToken);
        await using var transaction = await db.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);

        db.Add(vehicle);
        await db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return vehicle.Id;
    }

    public async Task<List<Vehicle>> GetAllVehicleAsync(CancellationToken cancellationToken)
    {
        await using var db = await contextFactory.CreateDbContextAsync(cancellationToken);
        return await db.Vehicles.AsNoTracking().ToListAsync(cancellationToken);
    }

    public async Task<Vehicle> GetVehicleByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        await using var db = await contextFactory.CreateDbContextAsync(cancellationToken);
        return await db.Vehicles.AsNoTracking().FirstAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<Vehicle?> GetVehicleByRegistrationNumberAsync(string registrationNumber, CancellationToken cancellationToken)
    {
        await using var db = await contextFactory.CreateDbContextAsync(cancellationToken);
        return await db.Vehicles
            .AsNoTracking()
            .FirstOrDefaultAsync(v => v.RegistrationNumber == registrationNumber, cancellationToken);
    }

    public async Task<List<TVehicle>> GetVehicleByTypeAsync<TVehicle>(CancellationToken cancellationToken) where TVehicle : Vehicle
    {
        await using var db = await contextFactory.CreateDbContextAsync(cancellationToken);
        return await db.Vehicles.AsNoTracking().OfType<TVehicle>().ToListAsync(cancellationToken);
    }
}
