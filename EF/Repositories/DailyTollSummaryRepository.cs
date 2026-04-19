using Entities.Interfaces;
using Entities.Tolls;
using Microsoft.EntityFrameworkCore;

namespace EFCore.Repositories;

public class DailyTollSummaryRepository(IDbContextFactory<TollDbContext> contextFactory) : IDailyTollSummaryRepository
{
    public async Task<Guid> CreateDailyTollSummaryAsync(DailyTollSummary dailyTollSummary, CancellationToken cancellationToken)
    {
        await using var db = await contextFactory.CreateDbContextAsync(cancellationToken);

        await db.AddAsync(dailyTollSummary, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);

        return dailyTollSummary.Id;
    }

    public async Task<bool> ExistsAsync(Guid vehicleId, DateOnly forDay, CancellationToken cancellationToken)
    {
        await using var db = await contextFactory.CreateDbContextAsync(cancellationToken);

        return await db.DailyTollSummaries
            .AsNoTracking()
            .AnyAsync(dts => dts.VehicleId == vehicleId && dts.ForDay == forDay, cancellationToken);
    }

    public async Task<List<DailyTollSummary>> GetAllByVehicleIdAsync(Guid vehicleId, CancellationToken cancellationToken)
    {
        await using var db = await contextFactory.CreateDbContextAsync(cancellationToken);

        return await db.DailyTollSummaries
            .AsNoTracking()
            .Where(dts => dts.VehicleId == vehicleId)
            .ToListAsync(cancellationToken);
    }
}
