using Entities.Interfaces;
using NSubstitute;
using UseCases.HelperClass;
using UseCases.Interfaces;

namespace Application.UseCases.Tests;

public class TollCalculatorTests
{
    private readonly ISwedishHolidayService _holidayService = Substitute.For<ISwedishHolidayService>();
    private readonly ITollRateService _tollRateService = new GothenburgTollRateService();
    private readonly IVehicle _car;
    private readonly IVehicle _motorbike;
    private readonly TollCalculator _sut;

    public TollCalculatorTests()
    {
        _car = Substitute.For<IVehicle>();
        _car.GetVehicleType().Returns("Car");

        _motorbike = Substitute.For<IVehicle>();
        _motorbike.GetVehicleType().Returns("Motorbike");

        _sut = new TollCalculator(_holidayService, _tollRateService);
    }

    // --- Toll-free dates ---

    [Fact]
    public void CalculateDailyTotalFee_OnSaturday_ShouldReturnZero()
    {
        DateTime saturday = new(2025, 6, 7, 8, 0, 0);
        Assert.Equal(0, _sut.CalculateDailyTotalFee(_car, [saturday]));
    }

    [Fact]
    public void CalculateDailyTotalFee_OnSunday_ShouldReturnZero()
    {
        DateTime sunday = new(2025, 6, 8, 8, 0, 0);
        Assert.Equal(0, _sut.CalculateDailyTotalFee(_car, [sunday]));
    }

    [Fact]
    public void CalculateDailyTotalFee_DuringJuly_ShouldReturnZero()
    {
        DateTime julyWeekday = new(2025, 7, 1, 8, 0, 0);
        Assert.Equal(0, _sut.CalculateDailyTotalFee(_car, [julyWeekday]));
    }

    [Fact]
    public void CalculateDailyTotalFee_OnPublicHoliday_ShouldReturnZero()
    {
        DateTime date = new(2025, 1, 6, 8, 0, 0);
        _holidayService.IsPublicHoliday(DateOnly.FromDateTime(date)).Returns(true);

        Assert.Equal(0, _sut.CalculateDailyTotalFee(_car, [date]));
    }

    [Fact]
    public void CalculateDailyTotalFee_OnDayBeforePublicHoliday_ShouldReturnZero()
    {
        DateTime date = new(2025, 6, 5, 8, 0, 0);
        _holidayService.IsDayBeforePublicHoliday(DateOnly.FromDateTime(date)).Returns(true);

        Assert.Equal(0, _sut.CalculateDailyTotalFee(_car, [date]));
    }

    // --- Toll-free vehicles ---

    [Fact]
    public void CalculateDailyTotalFee_WithTollFreeVehicle_ShouldReturnZero()
    {
        DateTime weekday = new(2025, 6, 2, 8, 0, 0);
        Assert.Equal(0, _sut.CalculateDailyTotalFee(_motorbike, [weekday]));
    }

    [Fact]
    public void CalculateDailyTotalFee_NullVehicle_ShouldReturnFee()
    {
        DateTime date = new(2025, 6, 2, 7, 0, 0);
        Assert.Equal(18, _sut.CalculateDailyTotalFee(null!, [date]));
    }

    // --- Time-based fees ---

    [Theory]
    [InlineData(6, 0, 8)]
    [InlineData(6, 29, 8)]
    [InlineData(6, 30, 13)]
    [InlineData(6, 59, 13)]
    [InlineData(7, 0, 18)]
    [InlineData(7, 59, 18)]
    [InlineData(8, 0, 13)]
    [InlineData(8, 29, 13)]
    [InlineData(8, 30, 8)]
    [InlineData(9, 0, 8)]
    [InlineData(14, 59, 8)]
    [InlineData(15, 0, 13)]
    [InlineData(15, 29, 13)]
    [InlineData(15, 30, 18)]
    [InlineData(16, 0, 18)]
    [InlineData(16, 59, 18)]
    [InlineData(17, 0, 13)]
    [InlineData(17, 59, 13)]
    [InlineData(18, 0, 8)]
    [InlineData(18, 29, 8)]
    [InlineData(18, 30, 0)]
    [InlineData(5, 59, 0)]
    [InlineData(19, 0, 0)]
    public void CalculateDailyTotalFee_AtSpecificTime_ShouldReturnExpectedFee(int hour, int minute, int expectedFee)
    {
        DateTime date = new(2025, 6, 2, hour, minute, 0);
        Assert.Equal(expectedFee, _sut.CalculateDailyTotalFee(_car, [date]));
    }

    // --- Daily max 60 SEK ---

    [Fact]
    public void CalculateDailyTotalFee_MultiplePasses_ShouldNotExceed60()
    {
        DateTime[] dates =
        [
            new(2025, 6, 2, 7, 0, 0),
            new(2025, 6, 2, 8, 30, 0),
            new(2025, 6, 2, 15, 0, 0),
            new(2025, 6, 2, 16, 30, 0),
            new(2025, 6, 2, 17, 31, 0),
        ];

        Assert.Equal(60, _sut.CalculateDailyTotalFee(_car, dates));
    }

    // --- 60-minute rule ---

    [Fact]
    public void CalculateDailyTotalFee_TwoPassesWithin60Minutes_ShouldChargeOnlyHighest()
    {
        DateTime[] dates =
        [
            new(2025, 6, 2, 6, 15, 0),
            new(2025, 6, 2, 7, 0, 0),
        ];

        Assert.Equal(18, _sut.CalculateDailyTotalFee(_car, dates));
    }

    [Fact]
    public void CalculateDailyTotalFee_ThreePassesWithin60Minutes_ShouldChargeOnlyHighest()
    {
        DateTime[] dates =
        [
            new(2025, 6, 2, 6, 0, 0),
            new(2025, 6, 2, 6, 30, 0),
            new(2025, 6, 2, 6, 55, 0),
        ];

        Assert.Equal(13, _sut.CalculateDailyTotalFee(_car, dates));
    }

    [Fact]
    public void CalculateDailyTotalFee_PassAtExactly61MinutesApart_ShouldChargeAsSeparateIntervals()
    {
        DateTime[] dates =
        [
            new(2025, 6, 2, 6, 0, 0),
            new(2025, 6, 2, 7, 1, 0),
        ];

        Assert.Equal(26, _sut.CalculateDailyTotalFee(_car, dates));
    }

    [Fact]
    public void CalculateDailyTotalFee_PassAtExactly60MinutesApart_ShouldChargeOnlyHighest()
    {
        DateTime[] dates =
        [
            new(2025, 6, 2, 6, 0, 0),
            new(2025, 6, 2, 7, 0, 0),
        ];

        Assert.Equal(18, _sut.CalculateDailyTotalFee(_car, dates));
    }

    [Fact]
    public void CalculateDailyTotalFee_LowerFeeAfterHigherWithin60Minutes_ShouldKeepHighest()
    {
        DateTime[] dates =
        [
            new(2025, 6, 2, 7, 0, 0),
            new(2025, 6, 2, 7, 30, 0),
        ];

        Assert.Equal(18, _sut.CalculateDailyTotalFee(_car, dates));
    }

    [Fact]
    public void CalculateDailyTotalFee_TwoSeparateIntervalsBothWithMultiplePasses_ShouldSumHighestFromEach()
    {
        DateTime[] dates =
        [
            new(2025, 6, 2, 6, 0, 0),
            new(2025, 6, 2, 6, 30, 0),
            new(2025, 6, 2, 15, 0, 0),
            new(2025, 6, 2, 15, 30, 0),
        ];

        Assert.Equal(31, _sut.CalculateDailyTotalFee(_car, dates));
    }

    // --- Single passage ---

    [Fact]
    public void CalculateDailyTotalFee_SinglePassage_ShouldReturnThatFee()
    {
        DateTime[] dates = [new(2025, 6, 2, 7, 0, 0)];
        Assert.Equal(18, _sut.CalculateDailyTotalFee(_car, dates));
    }

    // --- All passes toll-free ---

    [Fact]
    public void CalculateDailyTotalFee_UnsortedDates_ShouldProduceSameResultAsSorted()
    {
        DateTime[] unsorted =
        [
            new(2025, 6, 2, 15, 30, 0), // 18
            new(2025, 6, 2, 7, 0, 0),   // 18
            new(2025, 6, 2, 8, 30, 0),  // 8
        ];

        DateTime[] sorted =
        [
            new(2025, 6, 2, 7, 0, 0),
            new(2025, 6, 2, 8, 30, 0),
            new(2025, 6, 2, 15, 30, 0),
        ];

        Assert.Equal(
            _sut.CalculateDailyTotalFee(_car, sorted),
            _sut.CalculateDailyTotalFee(_car, unsorted));
    }

    [Fact]
    public void CalculateDailyTotalFee_AllPassesOutsideTollHours_ShouldReturnZero()
    {
        DateTime[] dates =
        [
            new(2025, 6, 2, 5, 0, 0),
            new(2025, 6, 2, 19, 0, 0),
            new(2025, 6, 2, 23, 0, 0),
        ];

        Assert.Equal(0, _sut.CalculateDailyTotalFee(_car, dates));
    }

    [Fact]
    public void CalculateDailyTotalFee_AllPassesOnWeekend_ShouldReturnZero()
    {
        DateTime[] dates =
        [
            new(2025, 6, 7, 7, 0, 0),
            new(2025, 6, 7, 8, 0, 0),
            new(2025, 6, 7, 15, 30, 0),
        ];

        Assert.Equal(0, _sut.CalculateDailyTotalFee(_car, dates));
    }

    [Fact]
    public void CalculateDailyTotalFee_TollFreeVehicleMultiplePasses_ShouldReturnZero()
    {
        DateTime[] dates =
        [
            new(2025, 6, 2, 7, 0, 0),
            new(2025, 6, 2, 8, 30, 0),
            new(2025, 6, 2, 15, 30, 0),
        ];

        Assert.Equal(0, _sut.CalculateDailyTotalFee(_motorbike, dates));
    }

    // --- Fee under max ---

    [Fact]
    public void CalculateDailyTotalFee_TotalUnderMax_ShouldReturnExactTotal()
    {
        DateTime[] dates =
        [
            new(2025, 6, 2, 7, 0, 0),
            new(2025, 6, 2, 8, 30, 0),
            new(2025, 6, 2, 15, 30, 0),
            new(2025, 6, 2, 17, 0, 0),
        ];

        Assert.Equal(57, _sut.CalculateDailyTotalFee(_car, dates));
    }

    // --- Boundary times ---

    [Fact]
    public void CalculateDailyTotalFee_AtMidnight_ShouldReturnZero()
    {
        DateTime date = new(2025, 6, 2, 0, 0, 0);
        Assert.Equal(0, _sut.CalculateDailyTotalFee(_car, [date]));
    }

    [Fact]
    public void CalculateDailyTotalFee_At2359_ShouldReturnZero()
    {
        DateTime date = new(2025, 6, 2, 23, 59, 0);
        Assert.Equal(0, _sut.CalculateDailyTotalFee(_car, [date]));
    }

    // --- July boundary ---

    [Fact]
    public void CalculateDailyTotalFee_OnJune30_ShouldReturnFee()
    {
        DateTime date = new(2025, 6, 30, 7, 0, 0);
        Assert.Equal(18, _sut.CalculateDailyTotalFee(_car, [date]));
    }

    [Fact]
    public void CalculateDailyTotalFee_OnJuly31_ShouldReturnZero()
    {
        DateTime date = new(2025, 7, 31, 7, 0, 0);
        Assert.Equal(0, _sut.CalculateDailyTotalFee(_car, [date]));
    }

    [Fact]
    public void CalculateDailyTotalFee_OnAugust1_ShouldReturnFee()
    {
        DateTime date = new(2025, 8, 1, 7, 0, 0);
        Assert.Equal(18, _sut.CalculateDailyTotalFee(_car, [date]));
    }

    // --- Mixed toll-free and taxable ---

    [Fact]
    public void CalculateDailyTotalFee_MixOfTollFreeAndTaxableTimes_ShouldOnlySumTaxable()
    {
        DateTime[] dates =
        [
            new(2025, 6, 2, 5, 0, 0),
            new(2025, 6, 2, 7, 0, 0),
            new(2025, 6, 2, 19, 0, 0),
        ];

        Assert.Equal(18, _sut.CalculateDailyTotalFee(_car, dates));
    }

    // --- Vehicle types ---

    [Theory]
    [InlineData("Emergency")]
    [InlineData("Diplomat")]
    [InlineData("Foreign")]
    [InlineData("Military")]
    [InlineData("Tractor")]
    public void CalculateDailyTotalFee_WithExemptVehicleType_ShouldReturnZero(string vehicleType)
    {
        IVehicle vehicle = Substitute.For<IVehicle>();
        vehicle.GetVehicleType().Returns(vehicleType);
        DateTime date = new(2025, 6, 2, 7, 0, 0);

        Assert.Equal(0, _sut.CalculateDailyTotalFee(vehicle, [date]));
    }

    [Fact]
    public void CalculateDailyTotalFee_WithUnknownVehicleType_ShouldReturnFee()
    {
        IVehicle vehicle = Substitute.For<IVehicle>();
        vehicle.GetVehicleType().Returns("UnknownType");
        DateTime date = new(2025, 6, 2, 7, 0, 0);

        Assert.Equal(18, _sut.CalculateDailyTotalFee(vehicle, [date]));
    }

    // --- Empty / null ---

    [Fact]
    public void CalculateDailyTotalFee_EmptyDates_ShouldReturnZero()
    {
        Assert.Equal(0, _sut.CalculateDailyTotalFee(_car, []));
    }

    [Fact]
    public void CalculateDailyTotalFee_NullDates_ShouldThrowArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => _sut.CalculateDailyTotalFee(_car, null!));
    }

    [Fact]
    public void CalculateDailyTotalFee_SinglePassageExceedsMaxDailyFee_ShouldReturnMaxDailyFee()
    {
        var stubRateService = Substitute.For<ITollRateService>();
        stubRateService.MaxDailyFee.Returns(5);
        stubRateService.GetFeeForTime(Arg.Any<TimeOnly>()).Returns(18);

        var sut = new TollCalculator(_holidayService, stubRateService);

        int result = sut.CalculateDailyTotalFee(_car, [new DateTime(2024, 3, 4, 7, 30, 0)]);

        Assert.Equal(5, result);
    }
}
