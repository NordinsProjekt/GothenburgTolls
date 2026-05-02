using Entities.Tolls;
using UseCases.Results;

namespace UseCases.Interfaces;

public interface IDailyTollSummaryService
{
    Task<DailyTollSummary> CreateAsync(string registrationNumber, DateOnly forDay, CancellationToken cancellationToken);
    Task<BackfillResult> BackfillMissedAsync(CancellationToken cancellationToken);
    Task<List<DailyTollSummary>> GetAllByVehicleIdAsync(Guid vehicleId, CancellationToken cancellationToken);
    Task<List<DailyTollSummary>> GetAllUninvoicedAsync(CancellationToken cancellationToken);
    Task<List<DailyTollSummary>> GetUninvoicedByVehicleIdAsync(Guid vehicleId, CancellationToken cancellationToken);
}
