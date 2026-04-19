namespace Entities.Interfaces;


public interface IDailyTollSummaryRepository
{
    Task<List<DailyTollSummary>> GetAllByVehicleIdAsync(Guid vehicleId, CancellationToken cancellationToken);
    Task<Guid> CreateDailyTollSummaryAsync(DailyTollSummary dailyTollSummary, CancellationToken cancellationToken);
}