using Entities.Interfaces;
using Entities.Tolls;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace EFCore.Repositories;

public class TollEventRepository(IDbContextFactory<TollDbContext> contextFactory) : ITollEventRepository
{
    public async Task<Guid> CreateTollEventAsync(TollEvent tollEvent, CancellationToken cancellationToken)
    {
        await using var db = await contextFactory.CreateDbContextAsync(cancellationToken);
        await using var transaction = await db.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);

        await db.AddAsync(tollEvent, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return tollEvent.Id;
    }

    public async Task<List<TollEvent>> GetAllByRegistrationAsync(string registrationNumber, DateOnly eventDate, CancellationToken cancellationToken)
    {
        await using var db = await contextFactory.CreateDbContextAsync(cancellationToken);

        var dateRange = new SwedishDateRange(eventDate);

        var vehicleIds = db.Vehicles
            .Where(v => v.RegistrationNumber == registrationNumber)
            .Select(v => v.Id);

        return await db.TollEvents.AsNoTracking()
            .Where(te => te.VehicleId != null && vehicleIds.Contains(te.VehicleId.Value))
            .Where(te => te.DailyTollSummaryId == null)
            .Where(te => te.EventDateTime >= dateRange.Start && te.EventDateTime < dateRange.End)
            .ToListAsync(cancellationToken);
    }

    public async Task<TollEvent> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        await using var db = await contextFactory.CreateDbContextAsync(cancellationToken);

        return await db.TollEvents.AsNoTracking().SingleAsync(te => te.Id == id, cancellationToken);
    }

    public async Task<List<TollEvent>> GetRecentAsync(int count, CancellationToken cancellationToken)
    {
        await using var db = await contextFactory.CreateDbContextAsync(cancellationToken);

        return await db.TollEvents.AsNoTracking()
            .Include(te => te.Vehicle)
            .OrderByDescending(te => te.EventDateTime)
            .Take(count)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<TollEvent>> GetUnassignedBeforeDateAsync(DateOnly before, CancellationToken cancellationToken)
    {
        await using var db = await contextFactory.CreateDbContextAsync(cancellationToken);

        DateTimeOffset cutoff = new SwedishDateRange(before).Start;

        return await db.TollEvents.AsNoTracking()
            .Where(te => te.DailyTollSummaryId == null && te.EventDateTime < cutoff)
            .OrderBy(te => te.EventDateTime)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<TollEvent>> GetUnassignedAsync(int count, CancellationToken cancellationToken)
    {
        await using var db = await contextFactory.CreateDbContextAsync(cancellationToken);

        return await db.TollEvents.AsNoTracking()
            .Include(te => te.Vehicle)
            .Where(te => te.DailyTollSummaryId == null)
            .OrderByDescending(te => te.EventDateTime)
            .Take(count)
            .ToListAsync(cancellationToken);
    }
}