using Entities.Interfaces;
using Entities.Tolls;
using Microsoft.EntityFrameworkCore;

namespace EFCore.Repositories;

public class TollInvoiceRepository(IDbContextFactory<TollDbContext> contextFactory) : ITollInvoiceRepository
{
    public async Task<Guid> CreateTollInvoiceAsync(TollInvoice tollInvoice, IReadOnlyList<DailyTollSummary> summaries, CancellationToken cancellationToken)
    {
        await using var db = await contextFactory.CreateDbContextAsync(cancellationToken);

        await db.AddAsync(tollInvoice, cancellationToken);

        foreach (DailyTollSummary summary in summaries)
        {
            db.Entry(summary).State = EntityState.Modified;
        }

        await db.SaveChangesAsync(cancellationToken);

        return tollInvoice.Id;
    }

    public async Task<List<TollInvoice>> GetAllAsync(CancellationToken cancellationToken)
    {
        await using var db = await contextFactory.CreateDbContextAsync(cancellationToken);
        return await db.TollInvoices.AsNoTracking()
            .Include(ti => ti.Vehicle)
            .Include(ti => ti.TollSummary)
            .OrderByDescending(ti => ti.Created)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsAsync(Guid vehicleId, int year, int month, CancellationToken cancellationToken)
    {
        await using var db = await contextFactory.CreateDbContextAsync(cancellationToken);
        return await db.TollInvoices.AsNoTracking()
            .AnyAsync(ti => ti.VehicleId == vehicleId && ti.Year == year && ti.Month == month, cancellationToken);
    }
}
