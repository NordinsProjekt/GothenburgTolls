using UseCases.Validators;

namespace Application.UseCases.Tests;

public class TollInvoiceServiceValidatorTests
{
    // --- ValidateAndNormalizeRegistrationNumber ---

    [Fact]
    public void ValidateAndNormalizeRegistrationNumber_WithValidInput_ShouldReturnTrimmedValue()
    {
        string result = TollInvoiceServiceValidator.ValidateAndNormalizeRegistrationNumber("  ABC123  ");

        Assert.Equal("ABC123", result);
    }

    [Fact]
    public void ValidateAndNormalizeRegistrationNumber_WithNull_ShouldThrowArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            TollInvoiceServiceValidator.ValidateAndNormalizeRegistrationNumber(null!));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void ValidateAndNormalizeRegistrationNumber_WithEmptyOrWhitespace_ShouldThrowArgumentException(string regNr)
    {
        Assert.Throws<ArgumentException>(() =>
            TollInvoiceServiceValidator.ValidateAndNormalizeRegistrationNumber(regNr));
    }

    // --- ValidateYear ---

    [Fact]
    public void ValidateYear_WithCurrentYear_ShouldNotThrow()
    {
        var exception = Record.Exception(() =>
            TollInvoiceServiceValidator.ValidateYear(DateTimeOffset.UtcNow.Year));

        Assert.Null(exception);
    }

    [Fact]
    public void ValidateYear_With2013_ShouldNotThrow()
    {
        var exception = Record.Exception(() =>
            TollInvoiceServiceValidator.ValidateYear(2013));

        Assert.Null(exception);
    }

    [Fact]
    public void ValidateYear_WithYearBelow2013_ShouldThrowArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            TollInvoiceServiceValidator.ValidateYear(2012));
    }

    [Fact]
    public void ValidateYear_WithFutureYear_ShouldThrowArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            TollInvoiceServiceValidator.ValidateYear(DateTimeOffset.UtcNow.Year + 1));
    }

    [Fact]
    public void ValidateYear_WithInvalidYear_ShouldHaveCorrectParamName()
    {
        var ex = Assert.Throws<ArgumentOutOfRangeException>(() =>
            TollInvoiceServiceValidator.ValidateYear(2012));

        Assert.Equal("year", ex.ParamName);
    }

    // --- ValidateMonth ---

    [Theory]
    [InlineData(1)]
    [InlineData(6)]
    [InlineData(12)]
    public void ValidateMonth_WithValidMonth_ShouldNotThrow(int month)
    {
        var exception = Record.Exception(() =>
            TollInvoiceServiceValidator.ValidateMonth(month));

        Assert.Null(exception);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(13)]
    public void ValidateMonth_WithInvalidMonth_ShouldThrowArgumentOutOfRangeException(int month)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            TollInvoiceServiceValidator.ValidateMonth(month));
    }

    [Fact]
    public void ValidateMonth_WithInvalidMonth_ShouldHaveCorrectParamName()
    {
        var ex = Assert.Throws<ArgumentOutOfRangeException>(() =>
            TollInvoiceServiceValidator.ValidateMonth(0));

        Assert.Equal("month", ex.ParamName);
    }
}
