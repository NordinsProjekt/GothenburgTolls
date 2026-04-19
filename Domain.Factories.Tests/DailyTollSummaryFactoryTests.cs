using Factories;

namespace Domain.Factories.Tests;

public class DailyTollSummaryFactoryTests
{
    private static readonly DateOnly ValidDay = new(2025, 6, 15);
    private const decimal ValidAmount = 45m;

    [Fact]
    public void Create_WithValidInput_ShouldReturnSummaryWithGivenForDay()
    {
        var summary = DailyTollSummaryFactory.Create(ValidDay, ValidAmount, Guid.NewGuid());

        Assert.Equal(ValidDay, summary.ForDay);
    }

    [Fact]
    public void Create_WithValidInput_ShouldReturnSummaryWithGivenAmount()
    {
        var summary = DailyTollSummaryFactory.Create(ValidDay, ValidAmount, Guid.NewGuid());

        Assert.Equal(ValidAmount, summary.Amount);
    }

    [Fact]
    public void Create_WithValidInput_ShouldReturnSummaryWithGivenVehicleId()
    {
        var vehicleId = Guid.NewGuid();

        var summary = DailyTollSummaryFactory.Create(ValidDay, ValidAmount, vehicleId);

        Assert.Equal(vehicleId, summary.VehicleId);
    }

    [Fact]
    public void Create_WithZeroAmount_ShouldReturnSummary()
    {
        var summary = DailyTollSummaryFactory.Create(ValidDay, 0m, Guid.NewGuid());

        Assert.Equal(0m, summary.Amount);
    }

    [Fact]
    public void Create_WithDefaultForDay_ShouldThrowArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
            DailyTollSummaryFactory.Create(default, ValidAmount, Guid.NewGuid()));
    }

    [Fact]
    public void Create_WithDefaultForDay_ShouldThrowWithForDayParamName()
    {
        var ex = Assert.Throws<ArgumentException>(() =>
            DailyTollSummaryFactory.Create(default, ValidAmount, Guid.NewGuid()));

        Assert.Equal("forDay", ex.ParamName);
    }

    [Fact]
    public void Create_WithNegativeAmount_ShouldThrowArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            DailyTollSummaryFactory.Create(ValidDay, -1m, Guid.NewGuid()));
    }

    [Fact]
    public void Create_WithNegativeAmount_ShouldThrowWithAmountParamName()
    {
        var ex = Assert.Throws<ArgumentOutOfRangeException>(() =>
            DailyTollSummaryFactory.Create(ValidDay, -1m, Guid.NewGuid()));

        Assert.Equal("amount", ex.ParamName);
    }

    [Fact]
    public void Create_WithEmptyVehicleId_ShouldThrowArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
            DailyTollSummaryFactory.Create(ValidDay, ValidAmount, Guid.Empty));
    }

    [Fact]
    public void Create_WithEmptyVehicleId_ShouldThrowWithVehicleIdParamName()
    {
        var ex = Assert.Throws<ArgumentException>(() =>
            DailyTollSummaryFactory.Create(ValidDay, ValidAmount, Guid.Empty));

        Assert.Equal("vehicleId", ex.ParamName);
    }
}
