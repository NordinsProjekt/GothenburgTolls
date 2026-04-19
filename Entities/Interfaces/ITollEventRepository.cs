using Entities.Tolls;

namespace Entities.Interfaces;

public interface ITollEventRepository
{
    Task<List<TollEvent>> GetAllByRegistrationAsync(string registrationNumber, DateOnly eventDate, CancellationToken cancellationToken);
    Task<TollEvent> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<Guid> CreateTollEventAsync(TollEvent tollEvent, CancellationToken cancellationToken);
}
