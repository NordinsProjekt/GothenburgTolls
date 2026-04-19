using UseCases.Dtos;

namespace UseCases.Validators;

internal static class VehicleServiceValidator
{
    internal static void ValidateDto(VehiclePassageDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);
    }
}
