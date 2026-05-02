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
        DateTimeOffset saturday = new DateTimeOffset(2025, 6, 7, 8, 0, 0, TimeSpan.Zero);
        Assert.Equal(0, _sut.CalculateDailyTotalFee(_car, [saturday]));
    }

    [Fact]
    public void CalculateDailyTotalFee_OnSunday_ShouldReturnZero()
    {
        DateTimeOffset sunday = new DateTimeOffset(2025, 6, 8, 8, 0, 0, TimeSpan.Zero);
        Assert.Equal(0, _sut.CalculateDailyTotalFee(_car, [sunday]));
    }

    [Fact]
    public void CalculateDailyTotalFee_DuringJuly_ShouldReturnZero()
    {
        DateTimeOffset julyWeekday = new DateTimeOffset(2025, 7, 1, 8, 0, 0, TimeSpan.Zero);
        Assert.Equal(0, _sut.CalculateDailyTotalFee(_car, [julyWeekday]));
    }

    [Fact]
    public void CalculateDailyTotalFee_OnPublicHoliday_ShouldReturnZero()
    {
        DateTimeOffset date = new DateTimeOffset(2025, 1, 6, 8, 0, 0, TimeSpan.Zero);
        _holidayService.IsPublicHoliday(DateOnly.FromDateTime(date.DateTime)).Returns(true);

        Assert.Equal(0, _sut.CalculateDailyTotalFee(_car, [date]));
    }

    [Fact]
    public void CalculateDailyTotalFee_OnDayBeforePublicHoliday_ShouldReturnZero()
    {
        DateTimeOffset date = new DateTimeOffset(2025, 6, 5, 8, 0, 0, TimeSpan.Zero);
        _holidayService.IsDayBeforePublicHoliday(DateOnly.FromDateTime(date.DateTime)).Returns(true);

        Assert.Equal(0, _sut.CalculateDailyTotalFee(_car, [date]));
    }

    // --- Toll-free vehicles ---

    [Fact]
    public void CalculateDailyTotalFee_WithTollFreeVehicle_ShouldReturnZero()
    {
        DateTimeOffset weekday = new DateTimeOffset(2025, 6, 2, 8, 0, 0, TimeSpan.Zero);
        Assert.Equal(0, _sut.CalculateDailyTotalFee(_motorbike, [weekday]));
    }

    [Fact]
    public void CalculateDailyTotalFee_NullVehicle_ShouldReturnFee()
    {
        DateTimeOffset date = new DateTimeOffset(2025, 6, 2, 7, 0, 0, TimeSpan.Zero);
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
        DateTimeOffset date = new(2025, 6, 2, hour, minute, 0, TimeSpan.Zero);
        Assert.Equal(expectedFee, _sut.CalculateDailyTotalFee(_car, [date]));
    }

    // --- Daily max 60 SEK ---

    [Fact]
    public void CalculateDailyTotalFee_MultiplePasses_ShouldNotExceed60()
    {
        DateTimeOffset[] dates =
        [
            new DateTimeOffset(2025, 6, 2, 7, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2025, 6, 2, 8, 30, 0, TimeSpan.Zero),
            new DateTimeOffset(2025, 6, 2, 15, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2025, 6, 2, 16, 30, 0, TimeSpan.Zero),
            new DateTimeOffset(2025, 6, 2, 17, 31, 0, TimeSpan.Zero),
        ];

        Assert.Equal(60, _sut.CalculateDailyTotalFee(_car, dates));
    }

    // --- 60-minute rule ---

    [Fact]
    public void CalculateDailyTotalFee_TwoPassesWithin60Minutes_ShouldChargeOnlyHighest()
    {
        DateTimeOffset[] dates =
        [
            new DateTimeOffset(2025, 6, 2, 6, 15, 0, TimeSpan.Zero),
            new DateTimeOffset(2025, 6, 2, 7, 0, 0, TimeSpan.Zero),
        ];

        Assert.Equal(18, _sut.CalculateDailyTotalFee(_car, dates));
    }

    [Fact]
    public void CalculateDailyTotalFee_ThreePassesWithin60Minutes_ShouldChargeOnlyHighest()
    {
        DateTimeOffset[] dates =
        [
            new DateTimeOffset(2025, 6, 2, 6, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2025, 6, 2, 6, 30, 0, TimeSpan.Zero),
            new DateTimeOffset(2025, 6, 2, 6, 55, 0, TimeSpan.Zero),
        ];

        Assert.Equal(13, _sut.CalculateDailyTotalFee(_car, dates));
    }

    [Fact]
    public void CalculateDailyTotalFee_PassAtExactly61MinutesApart_ShouldChargeAsSeparateIntervals()
    {
        DateTimeOffset[] dates =
        [
            new DateTimeOffset(2025, 6, 2, 6, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2025, 6, 2, 7, 1, 0, TimeSpan.Zero),
        ];

        Assert.Equal(26, _sut.CalculateDailyTotalFee(_car, dates));
    }

    [Fact]
    public void CalculateDailyTotalFee_PassAtExactly60MinutesApart_ShouldChargeOnlyHighest()
    {
        DateTimeOffset[] dates =
        [
            new DateTimeOffset(2025, 6, 2, 6, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2025, 6, 2, 7, 0, 0, TimeSpan.Zero),
        ];

        Assert.Equal(18, _sut.CalculateDailyTotalFee(_car, dates));
    }

    [Fact]
    public void CalculateDailyTotalFee_LowerFeeAfterHigherWithin60Minutes_ShouldKeepHighest()
    {
        DateTimeOffset[] dates =
        [
            new DateTimeOffset(2025, 6, 2, 7, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2025, 6, 2, 7, 30, 0, TimeSpan.Zero),
        ];

        Assert.Equal(18, _sut.CalculateDailyTotalFee(_car, dates));
    }

    [Fact]
    public void CalculateDailyTotalFee_TwoSeparateIntervalsBothWithMultiplePasses_ShouldSumHighestFromEach()
    {
        DateTimeOffset[] dates =
        [
            new DateTimeOffset(2025, 6, 2, 6, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2025, 6, 2, 6, 30, 0, TimeSpan.Zero),
            new DateTimeOffset(2025, 6, 2, 15, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2025, 6, 2, 15, 30, 0, TimeSpan.Zero),
        ];

        Assert.Equal(31, _sut.CalculateDailyTotalFee(_car, dates));
    }

    // --- Single passage ---

    [Fact]
    public void CalculateDailyTotalFee_SinglePassage_ShouldReturnThatFee()
    {
        DateTimeOffset[] dates = [new DateTimeOffset(2025, 6, 2, 7, 0, 0, TimeSpan.Zero)];
        Assert.Equal(18, _sut.CalculateDailyTotalFee(_car, dates));
    }

    // --- All passes toll-free ---

    [Fact]
    public void CalculateDailyTotalFee_UnsortedDates_ShouldProduceSameResultAsSorted()
    {
        DateTimeOffset[] unsorted =
        [
            new DateTimeOffset(2025, 6, 2, 15, 30, 0, TimeSpan.Zero), // 18
            new DateTimeOffset(2025, 6, 2, 7, 0, 0, TimeSpan.Zero),   // 18
            new DateTimeOffset(2025, 6, 2, 8, 30, 0, TimeSpan.Zero),  // 8
        ];

        DateTimeOffset[] sorted =
        [
            new DateTimeOffset(2025, 6, 2, 7, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2025, 6, 2, 8, 30, 0, TimeSpan.Zero),
            new DateTimeOffset(2025, 6, 2, 15, 30, 0, TimeSpan.Zero),
        ];

        Assert.Equal(
            _sut.CalculateDailyTotalFee(_car, sorted),
            _sut.CalculateDailyTotalFee(_car, unsorted));
    }

    [Fact]
    public void CalculateDailyTotalFee_AllPassesOutsideTollHours_ShouldReturnZero()
    {
        DateTimeOffset[] dates =
        [
            new DateTimeOffset(2025, 6, 2, 5, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2025, 6, 2, 19, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2025, 6, 2, 23, 0, 0, TimeSpan.Zero),
        ];

        Assert.Equal(0, _sut.CalculateDailyTotalFee(_car, dates));
    }

    [Fact]
    public void CalculateDailyTotalFee_AllPassesOnWeekend_ShouldReturnZero()
    {
        DateTimeOffset[] dates =
        [
            new DateTimeOffset(2025, 6, 7, 7, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2025, 6, 7, 8, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2025, 6, 7, 15, 30, 0, TimeSpan.Zero),
        ];

        Assert.Equal(0, _sut.CalculateDailyTotalFee(_car, dates));
    }

    [Fact]
    public void CalculateDailyTotalFee_TollFreeVehicleMultiplePasses_ShouldReturnZero()
    {
        DateTimeOffset[] dates =
        [
            new DateTimeOffset(2025, 6, 2, 7, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2025, 6, 2, 8, 30, 0, TimeSpan.Zero),
            new DateTimeOffset(2025, 6, 2, 15, 30, 0, TimeSpan.Zero),
        ];

        Assert.Equal(0, _sut.CalculateDailyTotalFee(_motorbike, dates));
    }

    // --- Fee under max ---

    [Fact]
    public void CalculateDailyTotalFee_TotalUnderMax_ShouldReturnExactTotal()
    {
        DateTimeOffset[] dates =
        [
            new DateTimeOffset(2025, 6, 2, 7, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2025, 6, 2, 8, 30, 0, TimeSpan.Zero),
            new DateTimeOffset(2025, 6, 2, 15, 30, 0, TimeSpan.Zero),
            new DateTimeOffset(2025, 6, 2, 17, 0, 0, TimeSpan.Zero),
        ];

        Assert.Equal(57, _sut.CalculateDailyTotalFee(_car, dates));
    }

    // --- Boundary times ---

    [Fact]
    public void CalculateDailyTotalFee_AtMidnight_ShouldReturnZero()
    {
        DateTimeOffset date = new DateTimeOffset(2025, 6, 2, 0, 0, 0, TimeSpan.Zero);
        Assert.Equal(0, _sut.CalculateDailyTotalFee(_car, [date]));
    }

    [Fact]
    public void CalculateDailyTotalFee_At2359_ShouldReturnZero()
    {
        DateTimeOffset date = new DateTimeOffset(2025, 6, 2, 23, 59, 0, TimeSpan.Zero);
        Assert.Equal(0, _sut.CalculateDailyTotalFee(_car, [date]));
    }

    // --- July boundary ---

    [Fact]
    public void CalculateDailyTotalFee_OnJune30_ShouldReturnFee()
    {
        DateTimeOffset date = new DateTimeOffset(2025, 6, 30, 7, 0, 0, TimeSpan.Zero);
        Assert.Equal(18, _sut.CalculateDailyTotalFee(_car, [date]));
    }

    [Fact]
    public void CalculateDailyTotalFee_OnJuly31_ShouldReturnZero()
    {
        DateTimeOffset date = new DateTimeOffset(2025, 7, 31, 7, 0, 0, TimeSpan.Zero);
        Assert.Equal(0, _sut.CalculateDailyTotalFee(_car, [date]));
    }

    [Fact]
    public void CalculateDailyTotalFee_OnAugust1_ShouldReturnFee()
    {
        DateTimeOffset date = new DateTimeOffset(2025, 8, 1, 7, 0, 0, TimeSpan.Zero);
        Assert.Equal(18, _sut.CalculateDailyTotalFee(_car, [date]));
    }

    // --- Mixed toll-free and taxable ---

    [Fact]
    public void CalculateDailyTotalFee_MixOfTollFreeAndTaxableTimes_ShouldOnlySumTaxable()
    {
        DateTimeOffset[] dates =
        [
            new DateTimeOffset(2025, 6, 2, 5, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2025, 6, 2, 7, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2025, 6, 2, 19, 0, 0, TimeSpan.Zero),
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
        DateTimeOffset date = new DateTimeOffset(2025, 6, 2, 7, 0, 0, TimeSpan.Zero);

        Assert.Equal(0, _sut.CalculateDailyTotalFee(vehicle, [date]));
    }

    [Fact]
    public void CalculateDailyTotalFee_WithUnknownVehicleType_ShouldReturnFee()
    {
        IVehicle vehicle = Substitute.For<IVehicle>();
        vehicle.GetVehicleType().Returns("UnknownType");
        DateTimeOffset date = new DateTimeOffset(2025, 6, 2, 7, 0, 0, TimeSpan.Zero);

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

        int result = sut.CalculateDailyTotalFee(_car, [new DateTimeOffset(2024, 3, 4, 7, 30, 0, TimeSpan.Zero)]);

        Assert.Equal(5, result);
    }

    // --- 10 passages every 30 minutes ---

    [Fact]
    public void CalculateDailyTotalFee_TenPassages30MinutesApart_ShouldChargeHighestPerInterval()
    {
        // Interval 1: 06:00(8), 06:30(13), 07:00(18) → 18
        // Interval 2: 07:30(18), 08:00(13), 08:30(8)  → 18
        // Interval 3: 09:00(8),  09:30(8),  10:00(8)   → 8
        // Interval 4: 10:30(8)                          → 8
        // Total: 18 + 18 + 8 + 8 = 52
        DateTimeOffset[] dates =
        [
            new DateTimeOffset(2025, 6, 2, 6, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2025, 6, 2, 6, 30, 0, TimeSpan.Zero),
            new DateTimeOffset(2025, 6, 2, 7, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2025, 6, 2, 7, 30, 0, TimeSpan.Zero),
            new DateTimeOffset(2025, 6, 2, 8, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2025, 6, 2, 8, 30, 0, TimeSpan.Zero),
            new DateTimeOffset(2025, 6, 2, 9, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2025, 6, 2, 9, 30, 0, TimeSpan.Zero),
            new DateTimeOffset(2025, 6, 2, 10, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2025, 6, 2, 10, 30, 0, TimeSpan.Zero),
        ];

        Assert.Equal(52, _sut.CalculateDailyTotalFee(_car, dates));
    }
}
