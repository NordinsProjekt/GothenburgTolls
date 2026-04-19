using Entities.Tolls;

namespace Entities.Interfaces;

public interface ITollInvoiceRepository
{
    Task<Guid> CreateTollInvoiceAsync(TollInvoice tollInvoice, IReadOnlyList<DailyTollSummary> summaries, CancellationToken cancellationToken);
    Task<List<TollInvoice>> GetAllAsync(CancellationToken cancellationToken);
    Task<bool> ExistsAsync(Guid vehicleId, int year, int month, CancellationToken cancellationToken);
}