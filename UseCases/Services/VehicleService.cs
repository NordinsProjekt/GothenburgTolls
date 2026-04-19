using Entities.Bases;
using Entities.Interfaces;
using Factories;
using UseCases.Dtos;
using UseCases.Interfaces;
using UseCases.Validators;

namespace UseCases.Services;

public class VehicleService(IVehicleRepository vehicleRepository) : IVehicleService
{
    public async Task<Vehicle> GetOrCreateAsync(VehiclePassageDto dto, CancellationToken cancellationToken)
    {
        VehicleServiceValidator.ValidateDto(dto);

        var existing = await vehicleRepository.GetVehicleByRegistrationNumberAsync(
            dto.RegistrationNumber, cancellationToken);

        if (existing is not null)
        {
            return existing;
        }

        var vehicle = VehicleFactory.Create(dto.RegistrationNumber, dto.VehicleType);
        await vehicleRepository.CreateVehicle(vehicle, cancellationToken);

        return vehicle;
    }
}
