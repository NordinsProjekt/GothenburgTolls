using Entities.Tolls;

namespace Entities.Interfaces;

public interface IDailyTollSummaryRepository
{
    Task<List<DailyTollSummary>> GetAllByVehicleIdAsync(Guid vehicleId, CancellationToken cancellationToken);
    Task<List<DailyTollSummary>> GetAllUninvoicedAsync(CancellationToken cancellationToken);
    Task<List<DailyTollSummary>> GetUninvoicedByVehicleIdAsync(Guid vehicleId, CancellationToken cancellationToken);
    Task<List<DailyTollSummary>> GetUninvoicedByVehicleAndMonthAsync(Guid vehicleId, int year, int month, CancellationToken cancellationToken);
    Task<bool> ExistsAsync(Guid vehicleId, DateOnly forDay, CancellationToken cancellationToken);
    Task<Guid> CreateWithTollEventsAsync(DailyTollSummary dailyTollSummary, IReadOnlyList<TollEvent> tollEvents, CancellationToken cancellationToken);
}