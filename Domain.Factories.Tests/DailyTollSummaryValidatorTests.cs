using Factories.Validator;

namespace Domain.Factories.Tests;

public class DailyTollSummaryValidatorTests
{
    [Fact]
    public void ValidateForDay_WithValidDate_ShouldNotThrow()
    {
        var exception = Record.Exception(() =>
            DailyTollSummaryValidator.ValidateForDay(new DateOnly(2025, 6, 15)));

        Assert.Null(exception);
    }

    [Fact]
    public void ValidateForDay_WithDefault_ShouldThrowArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
            DailyTollSummaryValidator.ValidateForDay(default));
    }

    [Fact]
    public void ValidateForDay_WithDefault_ShouldHaveCorrectParamName()
    {
        var ex = Assert.Throws<ArgumentException>(() =>
            DailyTollSummaryValidator.ValidateForDay(default));

        Assert.Equal("forDay", ex.ParamName);
    }

    [Fact]
    public void ValidateAmount_WithZero_ShouldNotThrow()
    {
        var exception = Record.Exception(() =>
            DailyTollSummaryValidator.ValidateAmount(0m));

        Assert.Null(exception);
    }

    [Fact]
    public void ValidateAmount_WithPositiveValue_ShouldNotThrow()
    {
        var exception = Record.Exception(() =>
            DailyTollSummaryValidator.ValidateAmount(100.50m));

        Assert.Null(exception);
    }

    [Fact]
    public void ValidateAmount_WithNegativeValue_ShouldThrowArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            DailyTollSummaryValidator.ValidateAmount(-1m));
    }

    [Fact]
    public void ValidateAmount_WithNegativeValue_ShouldHaveCorrectParamName()
    {
        var ex = Assert.Throws<ArgumentOutOfRangeException>(() =>
            DailyTollSummaryValidator.ValidateAmount(-1m));

        Assert.Equal("amount", ex.ParamName);
    }

    [Fact]
    public void ValidateVehicleId_WithValidId_ShouldNotThrow()
    {
        var exception = Record.Exception(() =>
            DailyTollSummaryValidator.ValidateVehicleId(Guid.NewGuid()));

        Assert.Null(exception);
    }

    [Fact]
    public void ValidateVehicleId_WithEmptyGuid_ShouldThrowArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
            DailyTollSummaryValidator.ValidateVehicleId(Guid.Empty));
    }

    [Fact]
    public void ValidateVehicleId_WithEmptyGuid_ShouldHaveCorrectParamName()
    {
        var ex = Assert.Throws<ArgumentException>(() =>
            DailyTollSummaryValidator.ValidateVehicleId(Guid.Empty));

        Assert.Equal("vehicleId", ex.ParamName);
    }
}
