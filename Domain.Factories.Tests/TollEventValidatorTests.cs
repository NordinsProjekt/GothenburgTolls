using Factories.Validator;

namespace Domain.Factories.Tests;

public class TollEventValidatorTests
{
    [Fact]
    public void ValidateEventDateTime_WithValidDateTime_ShouldNotThrow()
    {
        var exception = Record.Exception(() =>
            TollEventValidator.ValidateEventDateTime(DateTimeOffset.UtcNow.AddMinutes(-1)));

        Assert.Null(exception);
    }

    [Fact]
    public void ValidateEventDateTime_WithDefault_ShouldThrowArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
            TollEventValidator.ValidateEventDateTime(default));
    }

    [Fact]
    public void ValidateEventDateTime_WithDefault_ShouldHaveCorrectParamName()
    {
        var ex = Assert.Throws<ArgumentException>(() =>
            TollEventValidator.ValidateEventDateTime(default));

        Assert.Equal("eventDateTime", ex.ParamName);
    }

    [Fact]
    public void ValidateEventDateTime_WithFutureDate_ShouldThrowArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            TollEventValidator.ValidateEventDateTime(DateTimeOffset.UtcNow.AddHours(1)));
    }

    [Fact]
    public void ValidateAndNormalizeZone_WithValidZone_ShouldReturnTrimmedValue()
    {
        string result = TollEventValidator.ValidateAndNormalizeZone("  ZoneA  ");

        Assert.Equal("ZoneA", result);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void ValidateAndNormalizeZone_WithInvalidZone_ShouldThrowArgumentException(string? zone)
    {
        Assert.Throws<ArgumentException>(() =>
            TollEventValidator.ValidateAndNormalizeZone(zone!));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void ValidateAndNormalizeZone_WithInvalidZone_ShouldHaveCorrectParamName(string? zone)
    {
        var ex = Assert.Throws<ArgumentException>(() =>
            TollEventValidator.ValidateAndNormalizeZone(zone!));

        Assert.Equal("zone", ex.ParamName);
    }

    [Fact]
    public void ValidateAndNormalizeZone_WithTooLongZone_ShouldThrowArgumentOutOfRangeException()
    {
        string longZone = new('A', TollEventValidator.ZoneMaxLength + 1);

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            TollEventValidator.ValidateAndNormalizeZone(longZone));
    }

    [Fact]
    public void ValidateAndNormalizeZone_WithExactMaxLength_ShouldNotThrow()
    {
        string zone = new('A', TollEventValidator.ZoneMaxLength);

        var exception = Record.Exception(() => TollEventValidator.ValidateAndNormalizeZone(zone));

        Assert.Null(exception);
    }

    [Fact]
    public void ValidateVehicleId_WithValidId_ShouldNotThrow()
    {
        var exception = Record.Exception(() =>
            TollEventValidator.ValidateVehicleId(Guid.NewGuid()));

        Assert.Null(exception);
    }

    [Fact]
    public void ValidateVehicleId_WithEmptyGuid_ShouldThrowArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
            TollEventValidator.ValidateVehicleId(Guid.Empty));
    }

    [Fact]
    public void ValidateVehicleId_WithEmptyGuid_ShouldHaveCorrectParamName()
    {
        var ex = Assert.Throws<ArgumentException>(() =>
            TollEventValidator.ValidateVehicleId(Guid.Empty));

        Assert.Equal("vehicleId", ex.ParamName);
    }
}
