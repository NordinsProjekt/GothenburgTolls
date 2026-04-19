using Entities.Interfaces;
using Entities.Tolls;
using Factories;
using UseCases.Dtos;
using UseCases.Interfaces;

namespace UseCases.Services;

public class TollEventService(
    IVehicleService vehicleService,
    ITollEventRepository tollEventRepository) : ITollEventService
{
    public async Task<TollEvent> RegisterAsync(VehiclePassageDto dto, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(dto);

        var vehicle = await vehicleService.GetOrCreateAsync(dto, cancellationToken);

        var tollEvent = TollEventFactory.Create(dto.EventDateTime, dto.Zone, vehicle.Id);

        await tollEventRepository.CreateTollEventAsync(tollEvent, cancellationToken);

        return tollEvent;
    }
}
