using Factories;

namespace Domain.Factories.Tests;

public class TollEventFactoryTests
{
    private const string ValidZone = "ZoneA";

    [Fact]
    public void Create_WithValidInput_ShouldReturnTollEventWithGivenVehicleId()
    {
        var vehicleId = Guid.NewGuid();

        var tollEvent = TollEventFactory.Create(DateTime.UtcNow.AddMinutes(-1), ValidZone, vehicleId);

        Assert.Equal(vehicleId, tollEvent.VehicleId);
    }

    [Fact]
    public void Create_WithValidInput_ShouldReturnTollEventWithGivenEventDateTime()
    {
        var eventDateTime = DateTime.UtcNow.AddMinutes(-1);

        var tollEvent = TollEventFactory.Create(eventDateTime, ValidZone, Guid.NewGuid());

        Assert.Equal(eventDateTime, tollEvent.EventDateTime);
    }

    [Fact]
    public void Create_WithValidInput_ShouldReturnTollEventWithGivenZone()
    {
        var tollEvent = TollEventFactory.Create(DateTime.UtcNow.AddMinutes(-1), ValidZone, Guid.NewGuid());

        Assert.Equal(ValidZone, tollEvent.Zone);
    }

    [Fact]
    public void Create_WithZoneSurroundedByWhitespace_ShouldTrimZone()
    {
        var tollEvent = TollEventFactory.Create(DateTime.UtcNow.AddMinutes(-1), "  ZoneA  ", Guid.NewGuid());

        Assert.Equal("ZoneA", tollEvent.Zone);
    }

    [Fact]
    public void Create_WithDefaultEventDateTime_ShouldThrowArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
            TollEventFactory.Create(default, ValidZone, Guid.NewGuid()));
    }

    [Fact]
    public void Create_WithDefaultEventDateTime_ShouldThrowWithEventDateTimeParamName()
    {
        var ex = Assert.Throws<ArgumentException>(() =>
            TollEventFactory.Create(default, ValidZone, Guid.NewGuid()));

        Assert.Equal("eventDateTime", ex.ParamName);
    }

    [Fact]
    public void Create_WithFutureEventDateTime_ShouldThrowArgumentOutOfRangeException()
    {
        var future = DateTime.UtcNow.AddHours(1);

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            TollEventFactory.Create(future, ValidZone, Guid.NewGuid()));
    }

    [Fact]
    public void Create_WithFutureEventDateTime_ShouldExposeActualValueOnException()
    {
        var future = DateTimeOffset.UtcNow.AddHours(1);

        var ex = Assert.Throws<ArgumentOutOfRangeException>(() =>
            TollEventFactory.Create(future, ValidZone, Guid.NewGuid()));

        Assert.Equal(future, ex.ActualValue);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidZone_ShouldThrowArgumentException(string? zone)
    {
        Assert.Throws<ArgumentException>(() =>
            TollEventFactory.Create(DateTime.UtcNow.AddMinutes(-1), zone!, Guid.NewGuid()));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidZone_ShouldThrowWithZoneParamName(string? zone)
    {
        var ex = Assert.Throws<ArgumentException>(() =>
            TollEventFactory.Create(DateTime.UtcNow.AddMinutes(-1), zone!, Guid.NewGuid()));

        Assert.Equal("zone", ex.ParamName);
    }

    [Fact]
    public void Create_WithZoneExceeding64Characters_ShouldThrowArgumentOutOfRangeException()
    {
        var zone = new string('A', 65);

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            TollEventFactory.Create(DateTime.UtcNow.AddMinutes(-1), zone, Guid.NewGuid()));
    }

    [Fact]
    public void Create_WithZoneExactly64Characters_ShouldReturnTollEvent()
    {
        var zone = new string('A', 64);

        var tollEvent = TollEventFactory.Create(DateTime.UtcNow.AddMinutes(-1), zone, Guid.NewGuid());

        Assert.Equal(zone, tollEvent.Zone);
    }

    [Fact]
    public void Create_WithEmptyVehicleId_ShouldThrowArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
            TollEventFactory.Create(DateTime.UtcNow.AddMinutes(-1), ValidZone, Guid.Empty));
    }

    [Fact]
    public void Create_WithEmptyVehicleId_ShouldThrowWithVehicleIdParamName()
    {
        var ex = Assert.Throws<ArgumentException>(() =>
            TollEventFactory.Create(DateTime.UtcNow.AddMinutes(-1), ValidZone, Guid.Empty));

        Assert.Equal("vehicleId", ex.ParamName);
    }
}
