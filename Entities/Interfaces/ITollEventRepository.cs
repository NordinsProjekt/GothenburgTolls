using Entities.Tolls;

namespace Entities.Interfaces;

public interface ITollEventRepository
{
    Task<List<TollEvent>> GetAllByRegistrationAsync(string registrationNumber, DateOnly eventDate, CancellationToken cancellationToken);
    Task<List<TollEvent>> GetUnassignedBeforeDateAsync(DateOnly before, CancellationToken cancellationToken);
    Task<List<TollEvent>> GetUnassignedAsync(int count, CancellationToken cancellationToken);
    Task<TollEvent> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<List<TollEvent>> GetRecentAsync(int count, CancellationToken cancellationToken);
    Task<Guid> CreateTollEventAsync(TollEvent tollEvent, CancellationToken cancellationToken);
}
