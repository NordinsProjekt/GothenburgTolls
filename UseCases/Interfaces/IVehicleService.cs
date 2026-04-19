using Entities.Bases;
using UseCases.Dtos;

namespace UseCases.Interfaces;

public interface IVehicleService
{
    Task<Vehicle> GetOrCreateAsync(VehiclePassageDto dto, CancellationToken cancellationToken);
}
