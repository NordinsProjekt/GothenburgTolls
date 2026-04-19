using Entities.Types;

namespace UseCases.Dtos;

public record VehiclePassageDto(
    string RegistrationNumber,
    DateTime EventDateTime,
    string Zone,
    VehicleType VehicleType);
