using UseCases.HelperClass;

namespace Application.UseCases.Tests;

public class SwedishHolidayServiceTests
{
    private readonly SwedishHolidayService _sut = new();

    [Theory]
    [InlineData(2025, 1, 1)]   // Nyårsdagen
    [InlineData(2025, 1, 6)]   // Trettondedag jul
    [InlineData(2025, 5, 1)]   // Första maj
    [InlineData(2025, 6, 6)]   // Nationaldagen
    [InlineData(2025, 12, 24)] // Julafton
    [InlineData(2025, 12, 25)] // Juldagen
    [InlineData(2025, 12, 26)] // Annandag jul
    [InlineData(2025, 12, 31)] // Nyårsafton
    public void IsPublicHoliday_WithFixedHoliday_ShouldReturnTrue(int year, int month, int day)
    {
        Assert.True(_sut.IsPublicHoliday(new DateOnly(year, month, day)));
    }

    [Theory]
    [InlineData(2025, 4, 18)] // Långfredagen 2025
    [InlineData(2025, 4, 20)] // Påskdagen 2025
    [InlineData(2025, 4, 21)] // Annandag påsk 2025
    [InlineData(2025, 5, 29)] // Kristi himmelsfärdsdag 2025
    [InlineData(2025, 6, 8)]  // Pingstdagen 2025
    public void IsPublicHoliday_WithEasterBasedHoliday2025_ShouldReturnTrue(int year, int month, int day)
    {
        Assert.True(_sut.IsPublicHoliday(new DateOnly(year, month, day)));
    }

    [Theory]
    [InlineData(2024, 3, 29)] // Långfredagen 2024
    [InlineData(2024, 3, 31)] // Påskdagen 2024
    [InlineData(2024, 4, 1)]  // Annandag påsk 2024
    [InlineData(2024, 5, 9)]  // Kristi himmelsfärdsdag 2024
    [InlineData(2024, 5, 19)] // Pingstdagen 2024
    public void IsPublicHoliday_WithEasterBasedHoliday2024_ShouldReturnTrue(int year, int month, int day)
    {
        Assert.True(_sut.IsPublicHoliday(new DateOnly(year, month, day)));
    }

    [Fact]
    public void IsPublicHoliday_WithMidsommarafton2025_ShouldReturnTrue()
    {
        // Midsommarafton 2025: Friday June 20
        Assert.True(_sut.IsPublicHoliday(new DateOnly(2025, 6, 20)));
    }

    [Fact]
    public void IsPublicHoliday_WithMidsommardagen2025_ShouldReturnTrue()
    {
        // Midsommardagen 2025: Saturday June 21
        Assert.True(_sut.IsPublicHoliday(new DateOnly(2025, 6, 21)));
    }

    [Fact]
    public void IsPublicHoliday_WithAllaHelgonsDag2025_ShouldReturnTrue()
    {
        // Alla helgons dag 2025: Saturday November 1
        Assert.True(_sut.IsPublicHoliday(new DateOnly(2025, 11, 1)));
    }

    [Fact]
    public void IsPublicHoliday_WithAllaHelgonsDag2024_ShouldReturnTrue()
    {
        // Alla helgons dag 2024: Saturday November 2
        Assert.True(_sut.IsPublicHoliday(new DateOnly(2024, 11, 2)));
    }

    [Theory]
    [InlineData(2025, 2, 12)]  // Regular Wednesday
    [InlineData(2025, 3, 3)]   // Regular Monday
    [InlineData(2025, 9, 15)]  // Regular Monday
    public void IsPublicHoliday_WithRegularWorkday_ShouldReturnFalse(int year, int month, int day)
    {
        Assert.False(_sut.IsPublicHoliday(new DateOnly(year, month, day)));
    }

    [Fact]
    public void GetPublicHolidays_ForYear2025_ShouldReturnCorrectCount()
    {
        // 8 fixed + 5 easter-based + 2 midsommar + 1 alla helgons = 16
        var holidays = _sut.GetPublicHolidays(2025);

        Assert.Equal(16, holidays.Count);
    }

    [Fact]
    public void GetPublicHolidays_ForAnyYear_ShouldReturnSortedDates()
    {
        var holidays = _sut.GetPublicHolidays(2025);

        for (int i = 1; i < holidays.Count; i++)
        {
            Assert.True(holidays[i] >= holidays[i - 1]);
        }
    }

    // --- IsDayBeforePublicHoliday ---

    [Fact]
    public void IsDayBeforePublicHoliday_DayBeforeNyarsdagen_ShouldReturnTrue()
    {
        // Dec 31 is Nyårsafton (holiday) and Jan 1 is Nyårsdagen
        // Dec 30 → next day is Dec 31 (Nyårsafton, a holiday)
        Assert.True(_sut.IsDayBeforePublicHoliday(new DateOnly(2025, 12, 30)));
    }

    [Fact]
    public void IsDayBeforePublicHoliday_DayBeforeNationaldagen_ShouldReturnTrue()
    {
        Assert.True(_sut.IsDayBeforePublicHoliday(new DateOnly(2025, 6, 5)));
    }

    [Fact]
    public void IsDayBeforePublicHoliday_RegularDay_ShouldReturnFalse()
    {
        Assert.False(_sut.IsDayBeforePublicHoliday(new DateOnly(2025, 3, 3)));
    }

    // --- 2013 validation: verify all dates from original hardcoded TollCalculator ---

    [Theory]
    [InlineData(2013, 1, 1)]   // Nyårsdagen
    [InlineData(2013, 3, 29)]  // Långfredagen
    [InlineData(2013, 4, 1)]   // Annandag påsk
    [InlineData(2013, 5, 1)]   // Första maj
    [InlineData(2013, 5, 9)]   // Kristi himmelsfärdsdag
    [InlineData(2013, 6, 6)]   // Nationaldagen
    [InlineData(2013, 6, 21)]  // Midsommarafton
    [InlineData(2013, 12, 24)] // Julafton
    [InlineData(2013, 12, 25)] // Juldagen
    [InlineData(2013, 12, 26)] // Annandag jul
    [InlineData(2013, 12, 31)] // Nyårsafton
    public void IsPublicHoliday_WithHardcoded2013Holiday_ShouldReturnTrue(int year, int month, int day)
    {
        Assert.True(_sut.IsPublicHoliday(new DateOnly(year, month, day)));
    }

    [Theory]
    [InlineData(2013, 3, 28)]  // Skärtorsdag (day before Långfredagen)
    [InlineData(2013, 4, 30)]  // Valborgsmässoafton (day before Första maj)
    [InlineData(2013, 5, 8)]   // Day before Kristi himmelsfärdsdag
    [InlineData(2013, 6, 5)]   // Day before Nationaldagen
    [InlineData(2013, 11, 1)]  // Day before Alla helgons dag (Nov 2)
    public void IsDayBeforePublicHoliday_WithHardcoded2013DayBefore_ShouldReturnTrue(int year, int month, int day)
    {
        Assert.True(_sut.IsDayBeforePublicHoliday(new DateOnly(year, month, day)));
    }
}
