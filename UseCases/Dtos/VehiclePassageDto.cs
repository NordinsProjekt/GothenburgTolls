using Entities.Types;

namespace UseCases.Dtos;

public record VehiclePassageDto(
    string RegistrationNumber,
    DateTimeOffset EventDateTime,
    string Zone,
    VehicleType VehicleType);
