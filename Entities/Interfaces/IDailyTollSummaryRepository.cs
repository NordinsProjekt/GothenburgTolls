using Entities.Tolls;

namespace Entities.Interfaces;

public interface IDailyTollSummaryRepository
{
    Task<List<DailyTollSummary>> GetAllByVehicleIdAsync(Guid vehicleId, CancellationToken cancellationToken);
    Task<bool> ExistsAsync(Guid vehicleId, DateOnly forDay, CancellationToken cancellationToken);
    Task<Guid> CreateWithTollEventsAsync(DailyTollSummary dailyTollSummary, IReadOnlyList<TollEvent> tollEvents, CancellationToken cancellationToken);
}