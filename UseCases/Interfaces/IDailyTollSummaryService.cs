using Entities.Tolls;

namespace UseCases.Interfaces;

public interface IDailyTollSummaryService
{
    Task<DailyTollSummary> CreateAsync(string registrationNumber, DateOnly forDay, CancellationToken cancellationToken);
    Task<List<DailyTollSummary>> GetAllByVehicleIdAsync(Guid vehicleId, CancellationToken cancellationToken);
    Task<List<DailyTollSummary>> GetAllUninvoicedAsync(CancellationToken cancellationToken);
    Task<List<DailyTollSummary>> GetUninvoicedByVehicleIdAsync(Guid vehicleId, CancellationToken cancellationToken);
}
