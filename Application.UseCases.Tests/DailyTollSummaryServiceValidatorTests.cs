using UseCases.Validators;

namespace Application.UseCases.Tests;

public class DailyTollSummaryServiceValidatorTests
{
    // --- ValidateAndNormalizeRegistrationNumber ---

    [Fact]
    public void ValidateAndNormalizeRegistrationNumber_WithValidInput_ShouldReturnTrimmedValue()
    {
        string result = DailyTollSummaryServiceValidator.ValidateAndNormalizeRegistrationNumber("  ABC123  ");

        Assert.Equal("ABC123", result);
    }

    [Fact]
    public void ValidateAndNormalizeRegistrationNumber_WithNull_ShouldThrowArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            DailyTollSummaryServiceValidator.ValidateAndNormalizeRegistrationNumber(null!));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void ValidateAndNormalizeRegistrationNumber_WithEmptyOrWhitespace_ShouldThrowArgumentException(string regNr)
    {
        Assert.Throws<ArgumentException>(() =>
            DailyTollSummaryServiceValidator.ValidateAndNormalizeRegistrationNumber(regNr));
    }

    // --- ValidateForDay ---

    [Fact]
    public void ValidateForDay_WithPastDate_ShouldNotThrow()
    {
        DateOnly yesterday = DateOnly.FromDateTime(DateTimeOffset.Now.Date.AddDays(-1));

        var exception = Record.Exception(() => DailyTollSummaryServiceValidator.ValidateForDay(yesterday));

        Assert.Null(exception);
    }

    [Fact]
    public void ValidateForDay_WithToday_ShouldThrowArgumentOutOfRangeException()
    {
        DateOnly today = DateOnly.FromDateTime(DateTimeOffset.Now.Date);

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            DailyTollSummaryServiceValidator.ValidateForDay(today));
    }

    [Fact]
    public void ValidateForDay_WithFutureDate_ShouldThrowArgumentOutOfRangeException()
    {
        DateOnly future = DateOnly.FromDateTime(DateTimeOffset.Now.Date.AddDays(1));

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            DailyTollSummaryServiceValidator.ValidateForDay(future));
    }

    [Fact]
    public void ValidateForDay_WithToday_ShouldHaveCorrectParamName()
    {
        DateOnly today = DateOnly.FromDateTime(DateTimeOffset.Now.Date);

        var ex = Assert.Throws<ArgumentOutOfRangeException>(() =>
            DailyTollSummaryServiceValidator.ValidateForDay(today));

        Assert.Equal("forDay", ex.ParamName);
    }

    // --- ValidateVehicleId ---

    [Fact]
    public void ValidateVehicleId_WithValidId_ShouldNotThrow()
    {
        var exception = Record.Exception(() =>
            DailyTollSummaryServiceValidator.ValidateVehicleId(Guid.NewGuid()));

        Assert.Null(exception);
    }

    [Fact]
    public void ValidateVehicleId_WithEmptyGuid_ShouldThrowArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            DailyTollSummaryServiceValidator.ValidateVehicleId(Guid.Empty));
    }
}
