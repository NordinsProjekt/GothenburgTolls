using UseCases.Dtos;
using UseCases.Validators;

namespace Application.UseCases.Tests;

public class VehicleServiceValidatorTests
{
    [Fact]
    public void ValidateDto_WithNull_ShouldThrowArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            VehicleServiceValidator.ValidateDto(null!));
    }

    [Fact]
    public void ValidateDto_WithValidDto_ShouldNotThrow()
    {
        var dto = new VehiclePassageDto("ABC123", DateTimeOffset.UtcNow.AddMinutes(-5), "ZoneA", Entities.Types.VehicleType.Car);

        var exception = Record.Exception(() => VehicleServiceValidator.ValidateDto(dto));

        Assert.Null(exception);
    }
}
