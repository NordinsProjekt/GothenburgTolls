using Entities.Types;
using Entities.Vehicels;
using Factories;

namespace Domain.Factories.Tests;

public class VehicleFactoryTests
{
    [Theory]
    [InlineData(VehicleType.Car, typeof(Car))]
    [InlineData(VehicleType.Motorbike, typeof(Motorbike))]
    [InlineData(VehicleType.Tractor, typeof(Tractor))]
    [InlineData(VehicleType.Emergency, typeof(Emergency))]
    [InlineData(VehicleType.Diplomat, typeof(Diplomat))]
    [InlineData(VehicleType.Foreign, typeof(Foreign))]
    [InlineData(VehicleType.Military, typeof(Military))]
    public void Create_CreatingACertainVehicleType_ShouldReturnConcreteType(VehicleType type, Type expected)
    {
        var vehicle = VehicleFactory.Create("ABC123", type);

        Assert.IsType(expected, vehicle);
    }

    [Fact]
    public void Create_CreatingAVehicle_ShouldAssignRegistrationNumber()
    {
        var vehicle = VehicleFactory.Create("ABC123", VehicleType.Car);

        Assert.Equal("ABC123", vehicle.RegistrationNumber);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidRegistrationNumber_ShouldThrowArgumentException(string? regNr)
    {
        Assert.Throws<ArgumentException>(() =>
            VehicleFactory.Create(regNr!, VehicleType.Car));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidRegistrationNumber_ShouldThrowWithRegistrationNumberParamName(string? regNr)
    {
        var ex = Assert.Throws<ArgumentException>(() =>
            VehicleFactory.Create(regNr!, VehicleType.Car));

        Assert.Equal("registrationNumber", ex.ParamName);
    }

    [Fact]
    public void Create_WithBadVehicleType_ShouldThrowArgumentOutOfRangeException()
    {
        var unknown = (VehicleType)999;

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            VehicleFactory.Create("ABC123", unknown));
    }

    [Fact]
    public void Create_WithBadVehicleType_ShouldIncludeNumericValueInExceptionMessage()
    {
        var unknown = (VehicleType)999;

        var ex = Assert.Throws<ArgumentOutOfRangeException>(() =>
            VehicleFactory.Create("ABC123", unknown));

        Assert.Contains("999", ex.Message);
    }

    [Fact]
    public void Create_WithBadVehicleType_ShouldExposeActualValueOnException()
    {
        var unknown = (VehicleType)999;

        var ex = Assert.Throws<ArgumentOutOfRangeException>(() =>
            VehicleFactory.Create("ABC123", unknown));

        Assert.Equal(unknown, ex.ActualValue);
    }
}
