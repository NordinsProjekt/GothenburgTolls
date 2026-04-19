using UseCases.Dtos;
using UseCases.Validators;

namespace Application.UseCases.Tests;

public class TollEventServiceValidatorTests
{
    // --- ValidateDto ---

    [Fact]
    public void ValidateDto_WithNull_ShouldThrowArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            TollEventServiceValidator.ValidateDto(null!));
    }

    [Fact]
    public void ValidateDto_WithValidDto_ShouldNotThrow()
    {
        var dto = new VehiclePassageDto("ABC123", DateTimeOffset.UtcNow.AddMinutes(-5), "ZoneA", Entities.Types.VehicleType.Car);

        var exception = Record.Exception(() => TollEventServiceValidator.ValidateDto(dto));

        Assert.Null(exception);
    }

    // --- ValidateCount ---

    [Fact]
    public void ValidateCount_WithPositiveValue_ShouldNotThrow()
    {
        var exception = Record.Exception(() => TollEventServiceValidator.ValidateCount(1));

        Assert.Null(exception);
    }

    [Fact]
    public void ValidateCount_WithZero_ShouldThrowArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            TollEventServiceValidator.ValidateCount(0));
    }

    [Fact]
    public void ValidateCount_WithNegativeValue_ShouldThrowArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            TollEventServiceValidator.ValidateCount(-1));
    }
}
