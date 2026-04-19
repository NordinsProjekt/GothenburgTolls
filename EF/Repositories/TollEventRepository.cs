using Entities;
using Entities.Interfaces;
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

        return await db.TollEvents.AsNoTracking()
            .Where(te => te.Vehicle != null &&
                te.Vehicle.RegistrationNumber.Equals(registrationNumber))
               .Where(te => te.EventDateTime.Date == eventDate.ToDateTime(TimeOnly.MinValue))
            .ToListAsync(cancellationToken);
    }

    public async Task<TollEvent> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        await using var db = await contextFactory.CreateDbContextAsync(cancellationToken);

        return await db.TollEvents.AsNoTracking().SingleAsync(te => te.Id == id, cancellationToken);
    }
}
