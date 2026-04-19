using Entities.Tolls;
using UseCases.Dtos;

namespace UseCases.Interfaces;

public interface ITollEventService
{
    Task<TollEvent> RegisterAsync(VehiclePassageDto dto, CancellationToken cancellationToken);
    Task<IReadOnlyList<TollEvent>> GetRecentAsync(int count, CancellationToken cancellationToken);
}
