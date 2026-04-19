using Entities.Interfaces;
using Entities.Tolls;
using Microsoft.EntityFrameworkCore;

namespace EFCore.Repositories;

public class TollInvoiceRepository(IDbContextFactory<TollDbContext> contextFactory) : ITollInvoiceRepository
{
    public async Task<Guid> CreateTollInvoiceAsync(TollInvoice tollInvoice, CancellationToken cancellationToken)
    {
        await using var db = await contextFactory.CreateDbContextAsync(cancellationToken);
        await db.AddAsync(tollInvoice, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);

        return tollInvoice.Id;
    }

    public async Task<List<TollInvoice>> GetTollInvoicesAsync(Guid vehicleId, CancellationToken cancellationToken)
    {
        await using var db = await contextFactory.CreateDbContextAsync(cancellationToken);
        return await db.TollInvoices.AsNoTracking()
            .Where(ti => ti.TollSummary.Any(ts => ts.VehicleId == vehicleId))
            .ToListAsync(cancellationToken);
    }
}
