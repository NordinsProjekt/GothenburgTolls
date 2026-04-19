using Entities.Tolls;

namespace Entities.Interfaces;

public interface ITollInvoiceRepository
{
    Task<Guid> CreateTollInvoiceAsync(TollInvoice tollInvoice, CancellationToken cancellationToken);
    Task<List<TollInvoice>> GetTollInvoicesAsync(Guid vehicleId, CancellationToken cancellationToken);
}