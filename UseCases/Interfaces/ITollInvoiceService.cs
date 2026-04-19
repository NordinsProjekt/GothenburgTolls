using Entities.Tolls;

namespace UseCases.Interfaces;

public interface ITollInvoiceService
{
    Task<TollInvoice> CreateAsync(string registrationNumber, int year, int month, CancellationToken cancellationToken);
    Task<List<TollInvoice>> GetAllAsync(CancellationToken cancellationToken);
}
