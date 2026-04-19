using UseCases.Dtos;

namespace UseCases.Validators;

internal static class TollEventServiceValidator
{
    internal static void ValidateDto(VehiclePassageDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);
    }

    internal static void ValidateCount(int count)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(count);
    }
}
