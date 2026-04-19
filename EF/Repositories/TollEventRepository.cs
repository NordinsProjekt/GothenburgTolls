using Entities.Interfaces;
using Entities.Tolls;
using Microsoft.EntityFrameworkCore;

namespace EFCore.Repositories;

public class TollEventRepository(IDbContextFactory<TollDbContext> contextFactory) : ITollEventRepository
{
    public async Task<Guid> CreateTollEventAsync(TollEvent tollEvent, CancellationToken cancellationToken)
    {
        await using var db = await contextFactory.CreateDbContextAsync(cancellationToken);
        await db.AddAsync(tollEvent, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);

        return tollEvent.Id;
    }

    public async Task<List<TollEvent>> GetAllByRegistrationAsync(string registrationNumber, DateOnly eventDate, CancellationToken cancellationToken)
    {
        await using var db = await contextFactory.CreateDbContextAsync(cancellationToken);

        var swedishTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Europe/Stockholm");
        var dayStartLocal = eventDate.ToDateTime(TimeOnly.MinValue);
        var nextDayStartLocal = eventDate.AddDays(1).ToDateTime(TimeOnly.MinValue);
        var dayStart = new DateTimeOffset(dayStartLocal, swedishTimeZone.GetUtcOffset(dayStartLocal));
        var nextDayStart = new DateTimeOffset(nextDayStartLocal, swedishTimeZone.GetUtcOffset(nextDayStartLocal));

        var vehicleIds = db.Vehicles
            .Where(v => v.RegistrationNumber == registrationNumber)
            .Select(v => v.Id);

        return await db.TollEvents.AsNoTracking()
            .Where(te => te.VehicleId != null && vehicleIds.Contains(te.VehicleId.Value))
            .Where(te => te.DailyTollSummaryId == null)
            .Where(te => te.EventDateTime >= dayStart && te.EventDateTime < nextDayStart)
            .ToListAsync(cancellationToken);
    }

    public async Task<TollEvent> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        await using var db = await contextFactory.CreateDbContextAsync(cancellationToken);

        return await db.TollEvents.AsNoTracking().SingleAsync(te => te.Id == id, cancellationToken);
    }

    public async Task<List<TollEvent>> GetRecentAsync(int count, CancellationToken cancellationToken)
    {
        if (count <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(count), count, "Count must be greater than zero.");
        }

        await using var db = await contextFactory.CreateDbContextAsync(cancellationToken);

        return await db.TollEvents.AsNoTracking()
            .Include(te => te.Vehicle)
            .OrderByDescending(te => te.EventDateTime)
            .Take(count)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<TollEvent>> GetUnassignedAsync(int count, CancellationToken cancellationToken)
    {
        if (count <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(count), count, "Count must be greater than zero.");
        }

        await using var db = await contextFactory.CreateDbContextAsync(cancellationToken);

        return await db.TollEvents.AsNoTracking()
            .Include(te => te.Vehicle)
            .Where(te => te.DailyTollSummaryId == null)
            .OrderByDescending(te => te.EventDateTime)
            .Take(count)
            .ToListAsync(cancellationToken);
    }
}