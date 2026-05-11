using Entities.Interfaces;
using Entities.Tolls;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace EFCore.Repositories;

public class TollEventRepository(IDbContextFactory<TollDbContext> contextFactory) : ITollEventRepository
{
    /// <summary>
    /// Persists a new <see cref="TollEvent"/> to the database inside a ReadCommitted transaction
    /// and returns the generated identifier.
    /// </summary>
    /// <param name="tollEvent">The toll event entity to persist.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>The <see cref="Guid"/> assigned to the created toll event.</returns>
    public async Task<Guid> CreateTollEventAsync(TollEvent tollEvent, CancellationToken cancellationToken)
    {
        await using var db = await contextFactory.CreateDbContextAsync(cancellationToken);
        await using var transaction = await db.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);

        await db.AddAsync(tollEvent, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return tollEvent.Id;
    }

    /// <summary>
    /// Returns all unassigned toll events for the given registration number that fall within
    /// the Swedish calendar day represented by <paramref name="eventDate"/>.
    /// Only events not yet linked to a <c>DailyTollSummary</c> are included.
    /// </summary>
    /// <param name="registrationNumber">Vehicle registration number to filter by.</param>
    /// <param name="eventDate">The date whose Swedish day boundary is used as the time range.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>A list of matching <see cref="TollEvent"/> entities, or an empty list if none are found.</returns>
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

    /// <summary>
    /// Returns the <see cref="TollEvent"/> with the specified identifier.
    /// Throws <see cref="InvalidOperationException"/> if no matching record exists.
    /// </summary>
    /// <param name="id">The unique identifier of the toll event.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>The matching <see cref="TollEvent"/>.</returns>
    /// <exception cref="InvalidOperationException">Thrown when no toll event with the given id is found.</exception>
    public async Task<TollEvent> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        await using var db = await contextFactory.CreateDbContextAsync(cancellationToken);

        return await db.TollEvents.AsNoTracking().SingleAsync(te => te.Id == id, cancellationToken);
    }

    /// <summary>
    /// Returns the most recent toll events ordered by <c>EventDateTime</c> descending,
    /// including the related <c>Vehicle</c> navigation property.
    /// </summary>
    /// <param name="count">Maximum number of toll events to return.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>A list of at most <paramref name="count"/> <see cref="TollEvent"/> entities.</returns>
    public async Task<List<TollEvent>> GetRecentAsync(int count, CancellationToken cancellationToken)
    {
        await using var db = await contextFactory.CreateDbContextAsync(cancellationToken);

        return await db.TollEvents.AsNoTracking()
            .Include(te => te.Vehicle)
            .OrderByDescending(te => te.EventDateTime)
            .Take(count)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Returns all toll events that have not yet been assigned to a <c>DailyTollSummary</c>
    /// and whose <c>EventDateTime</c> is before the start of the given date (Europe/Stockholm, CET/CEST).
    /// Results are ordered by <c>EventDateTime</c> ascending.
    /// </summary>
    /// <param name="before">Cutoff date; events before midnight of this date are included.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>A list of unassigned <see cref="TollEvent"/> entities older than <paramref name="before"/>.</returns>
    public async Task<List<TollEvent>> GetUnassignedBeforeDateAsync(DateOnly before, CancellationToken cancellationToken)
    {
        await using var db = await contextFactory.CreateDbContextAsync(cancellationToken);

        DateTimeOffset cutoff = new SwedishDateRange(before).Start;

        return await db.TollEvents.AsNoTracking()
            .Where(te => te.DailyTollSummaryId == null && te.EventDateTime < cutoff)
            .OrderBy(te => te.EventDateTime)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Returns the most recent unassigned toll events — those not yet linked to a <c>DailyTollSummary</c> —
    /// ordered by <c>EventDateTime</c> descending, including the related <c>Vehicle</c>.
    /// </summary>
    /// <param name="count">Maximum number of toll events to return.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>A list of at most <paramref name="count"/> unassigned <see cref="TollEvent"/> entities.</returns>
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

    /// <summary>
    /// Deletes the <see cref="TollEvent"/> with the specified identifier from the database.
    /// </summary>
    /// <param name="id">The unique identifier of the toll event to delete.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns><c>true</c> if the toll event was deleted; <c>false</c> if no matching record was found.</returns>
    public async Task<bool> DeleteTollEventAsync(Guid id, CancellationToken cancellationToken)
    {
        await using var db = await contextFactory.CreateDbContextAsync(cancellationToken);

        TollEvent? tollEvent = await db.TollEvents.FirstOrDefaultAsync(te => te.Id == id, cancellationToken);

        if (tollEvent is null)
        {
            return false;
        }

        db.Remove(tollEvent);
        await db.SaveChangesAsync(cancellationToken);

        return true;
    }
}