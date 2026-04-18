using Entities.Bases;

namespace Entities.Interfaces;

public interface IVehicleRepository
{
    Task<List<Vehicle>> GetAllVehicleAsync(CancellationToken cancellationToken);
    Task<Vehicle> GetVehicleByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<List<Vehicle>> GetVehicleByType(Type vehicleType, CancellationToken cancellationToken);

    Task<Guid> CreateVehicle(Vehicle vehicle, CancellationToken cancellationToken);
}