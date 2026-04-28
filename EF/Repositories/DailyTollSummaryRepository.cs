using Entities.Interfaces;
using Entities.Tolls;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace EFCore.Repositories;

public class DailyTollSummaryRepository(IDbContextFactory<TollDbContext> contextFactory) : IDailyTollSummaryRepository
{
    public async Task<Guid> CreateWithTollEventsAsync(DailyTollSummary dailyTollSummary, IReadOnlyList<TollEvent> tollEvents, CancellationToken cancellationToken)
    {
        await using var db = await contextFactory.CreateDbContextAsync(cancellationToken);
        await using var transaction = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);

        await db.AddAsync(dailyTollSummary, cancellationToken);
        db.TollEvents.UpdateRange(tollEvents);
        await db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

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

    public async Task<List<DailyTollSummary>> GetAllUninvoicedAsync(CancellationToken cancellationToken)
    {
        await using var db = await contextFactory.CreateDbContextAsync(cancellationToken);

        return await db.DailyTollSummaries
            .AsNoTracking()
            .Include(dts => dts.Vehicle)
            .Where(dts => dts.TollInvoiceId == null)
            .OrderBy(dts => dts.ForDay)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<DailyTollSummary>> GetUninvoicedByVehicleIdAsync(Guid vehicleId, CancellationToken cancellationToken)
    {
        await using var db = await contextFactory.CreateDbContextAsync(cancellationToken);

        return await db.DailyTollSummaries
            .AsNoTracking()
            .Where(dts => dts.VehicleId == vehicleId && dts.TollInvoiceId == null)
            .OrderBy(dts => dts.ForDay)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<DailyTollSummary>> GetUninvoicedByVehicleAndMonthAsync(Guid vehicleId, int year, int month, CancellationToken cancellationToken)
    {
        await using var db = await contextFactory.CreateDbContextAsync(cancellationToken);

        return await db.DailyTollSummaries
            .AsNoTracking()
            .Where(dts => dts.VehicleId == vehicleId
                && dts.TollInvoiceId == null
                && dts.ForDay.Year == year
                && dts.ForDay.Month == month)
            .OrderBy(dts => dts.ForDay)
            .ToListAsync(cancellationToken);
    }
}
