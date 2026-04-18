using Entities.Bases;
using Entities.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EF.Repositories;

public class VehicleRepository(IDbContextFactory<TollDbContext> contextFactory) : IVehicleRepository
{
    public async Task<Guid> CreateVehicle(Vehicle vehicle, CancellationToken cancellationToken)
    {
        await using var db = await contextFactory.CreateDbContextAsync(cancellationToken);

        db.Add(vehicle);
        await db.SaveChangesAsync(cancellationToken);
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

    public async Task<List<Vehicle>> GetVehicleByType(Type vehicleType, CancellationToken cancellationToken)
    {
        await using var db = await contextFactory.CreateDbContextAsync(cancellationToken);
        return await db.Vehicles.AsNoTracking().Where(v => v.GetType() == vehicleType).ToListAsync(cancellationToken);
    }
}
