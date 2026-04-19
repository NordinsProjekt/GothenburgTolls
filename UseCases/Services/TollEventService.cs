using Entities.Interfaces;
using Entities.Tolls;
using Factories;
using UseCases.Dtos;
using UseCases.Interfaces;
using UseCases.Validators;

namespace UseCases.Services;

public class TollEventService(
    IVehicleService vehicleService,
    ITollEventRepository tollEventRepository) : ITollEventService
{
    public async Task<TollEvent> RegisterAsync(VehiclePassageDto dto, CancellationToken cancellationToken)
    {
        TollEventServiceValidator.ValidateDto(dto);

        var vehicle = await vehicleService.GetOrCreateAsync(dto, cancellationToken);

        var tollEvent = TollEventFactory.Create(dto.EventDateTime, dto.Zone, vehicle.Id);

        await tollEventRepository.CreateTollEventAsync(tollEvent, cancellationToken);

        return tollEvent;
    }

    public async Task<IReadOnlyList<TollEvent>> GetRecentAsync(int count, CancellationToken cancellationToken)
    {
        TollEventServiceValidator.ValidateCount(count);
        var events = await tollEventRepository.GetRecentAsync(count, cancellationToken);
        return events;
    }

    public async Task<IReadOnlyList<TollEvent>> GetUnassignedAsync(int count, CancellationToken cancellationToken)
    {
        TollEventServiceValidator.ValidateCount(count);
        var events = await tollEventRepository.GetUnassignedAsync(count, cancellationToken);
        return events;
    }
}
