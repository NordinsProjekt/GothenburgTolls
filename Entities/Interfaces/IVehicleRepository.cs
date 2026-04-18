using Entities.Bases;
using Entities.Types;

namespace Entities.Interfaces;

public interface IVehicleRepository
{
    Task<Vehicle> GetAllVehicleAsync();
    Task<Vehicle> GetVehicleByIdAsync(Guid id);
    Task<Vehicle> GetVehicleByType(VehicleType type);

    Task<Guid> CreateVehicle(Vehicle vehicle);
}