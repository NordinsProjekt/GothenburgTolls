using Entities.Bases;

namespace Entities.Interfaces;

public interface IVehicleRepository
{
    Task<List<Vehicle>> GetAllVehicleAsync(CancellationToken cancellationToken);
    Task<Vehicle> GetVehicleByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<Vehicle?> GetVehicleByRegistrationNumberAsync(string registrationNumber, CancellationToken cancellationToken);
    Task<List<TVehicle>> GetVehicleByTypeAsync<TVehicle>(CancellationToken cancellationToken) where TVehicle : Vehicle;

    Task<Guid> CreateVehicle(Vehicle vehicle, CancellationToken cancellationToken);
}