using Entities;
using Entities.Interfaces;
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

    public async Task<List<DailyTollSummary>> GetAllByVehicleIdAsync(Guid vehicleId, CancellationToken cancellationToken)
    {
        await using var db = await contextFactory.CreateDbContextAsync(cancellationToken);

        return await db.DailyTollSummaries
            .AsNoTracking()
            .Where(dts => dts.VehicleId == vehicleId)
            .ToListAsync(cancellationToken);
    }
}
