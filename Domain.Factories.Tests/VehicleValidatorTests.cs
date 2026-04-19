using Entities.Types;
using Factories.Validator;

namespace Domain.Factories.Tests;

public class VehicleValidatorTests
{
    [Fact]
    public void ValidateAndNormalizeRegistrationNumber_WithValidInput_ShouldReturnTrimmedValue()
    {
        string result = VehicleValidator.ValidateAndNormalizeRegistrationNumber("  ABC123  ");

        Assert.Equal("ABC123", result);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void ValidateAndNormalizeRegistrationNumber_WithInvalidInput_ShouldThrowArgumentException(string? regNr)
    {
        Assert.Throws<ArgumentException>(() =>
            VehicleValidator.ValidateAndNormalizeRegistrationNumber(regNr!));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void ValidateAndNormalizeRegistrationNumber_WithInvalidInput_ShouldHaveCorrectParamName(string? regNr)
    {
        var ex = Assert.Throws<ArgumentException>(() =>
            VehicleValidator.ValidateAndNormalizeRegistrationNumber(regNr!));

        Assert.Equal("registrationNumber", ex.ParamName);
    }

    [Fact]
    public void ValidateVehicleType_WithDefinedType_ShouldNotThrow()
    {
        var exception = Record.Exception(() => VehicleValidator.ValidateVehicleType(VehicleType.Car));

        Assert.Null(exception);
    }

    [Fact]
    public void ValidateVehicleType_WithUndefinedType_ShouldThrowArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            VehicleValidator.ValidateVehicleType((VehicleType)999));
    }

    [Fact]
    public void ValidateVehicleType_WithUndefinedType_ShouldIncludeNumericValueInMessage()
    {
        var ex = Assert.Throws<ArgumentOutOfRangeException>(() =>
            VehicleValidator.ValidateVehicleType((VehicleType)999));

        Assert.Contains("999", ex.Message);
    }

    [Fact]
    public void ValidateVehicleType_WithUndefinedType_ShouldExposeActualValue()
    {
        var ex = Assert.Throws<ArgumentOutOfRangeException>(() =>
            VehicleValidator.ValidateVehicleType((VehicleType)999));

        Assert.Equal((VehicleType)999, ex.ActualValue);
    }
}
