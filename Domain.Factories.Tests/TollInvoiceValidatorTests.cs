using Entities.Tolls;
using Factories.Validator;

namespace Domain.Factories.Tests;

public class TollInvoiceValidatorTests
{
    [Fact]
    public void ValidateVehicleId_WithValidId_ShouldNotThrow()
    {
        var exception = Record.Exception(() =>
            TollInvoiceValidator.ValidateVehicleId(Guid.NewGuid()));

        Assert.Null(exception);
    }

    [Fact]
    public void ValidateVehicleId_WithEmptyGuid_ShouldThrowArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            TollInvoiceValidator.ValidateVehicleId(Guid.Empty));
    }

    [Fact]
    public void ValidateYear_WithPositiveValue_ShouldNotThrow()
    {
        var exception = Record.Exception(() =>
            TollInvoiceValidator.ValidateYear(2025));

        Assert.Null(exception);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void ValidateYear_WithZeroOrNegative_ShouldThrowArgumentOutOfRangeException(int year)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            TollInvoiceValidator.ValidateYear(year));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(6)]
    [InlineData(12)]
    public void ValidateMonth_WithValidMonth_ShouldNotThrow(int month)
    {
        var exception = Record.Exception(() =>
            TollInvoiceValidator.ValidateMonth(month));

        Assert.Null(exception);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(13)]
    public void ValidateMonth_WithInvalidMonth_ShouldThrowArgumentOutOfRangeException(int month)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            TollInvoiceValidator.ValidateMonth(month));
    }

    [Fact]
    public void ValidateMonth_WithInvalidMonth_ShouldHaveCorrectParamName()
    {
        var ex = Assert.Throws<ArgumentOutOfRangeException>(() =>
            TollInvoiceValidator.ValidateMonth(0));

        Assert.Equal("month", ex.ParamName);
    }

    [Fact]
    public void ValidateAndMaterializeSummaries_WithNull_ShouldThrowArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            TollInvoiceValidator.ValidateAndMaterializeSummaries(null!));
    }

    [Fact]
    public void ValidateAndMaterializeSummaries_WithEmptyList_ShouldThrowArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
            TollInvoiceValidator.ValidateAndMaterializeSummaries([]));
    }

    [Fact]
    public void ValidateAndMaterializeSummaries_WithItems_ShouldReturnList()
    {
        var summary = new DailyTollSummary(new DateOnly(2025, 1, 1), 100m, Guid.NewGuid());

        List<DailyTollSummary> result = TollInvoiceValidator.ValidateAndMaterializeSummaries([summary]);

        Assert.Single(result);
    }
}
