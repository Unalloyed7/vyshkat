using VyshKat.Core.Model;
using VyshKat.Core.Services;

namespace VyshKat.Tests;

public sealed class ModelTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void InventoryRejectsBlankNumberAndName(string? value)
    {
        Assert.ThrowsAny<ArgumentException>(() => new Helmet(value!, "Шлем"));
        Assert.ThrowsAny<ArgumentException>(() => new Helmet("H-1", value!));
    }

    [Fact]
    public void InventoryTrimsNumberAndName()
    {
        var helmet = new Helmet("  H-1  ", "  Шлем  ");
        Assert.Equal("H-1", helmet.InventoryNumber);
        Assert.Equal("Шлем", helmet.Name);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(0)]
    [InlineData(11)]
    public void TransportRejectsEaseOutsideOneToTen(int ease)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new Bicycle("B-1", "Велосипед", ease, true));
    }

    [Theory]
    [InlineData(1, false)]
    [InlineData(5, false)]
    [InlineData(6, true)]
    [InlineData(10, true)]
    public void BeginnerRuleUsesSixAsInclusiveBoundary(int ease, bool expected)
    {
        Assert.Equal(expected, new Bicycle("B-1", "Велосипед", ease, true).IsSuitableForBeginners);
    }

    [Fact]
    public void EnergyConsumersRejectNegativeEnergy()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new Scooter("S-1", "Самокат", 6, true, -1m));
        Assert.Throws<ArgumentOutOfRangeException>(() => new ElectricBicycle("E-1", "Электровелосипед", 6, true, -1m));
        Assert.Throws<ArgumentOutOfRangeException>(() => new ChargingStation("C-1", "Станция", -1m));
    }

    [Fact]
    public void ZeroEnergyIsAllowed()
    {
        Assert.Equal(0m, new Scooter("S-1", "Самокат", 6, true, 0m).DailyEnergyKwh);
        Assert.Equal(0m, new ChargingStation("C-1", "Станция", 0m).DailyEnergyKwh);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void ServiceCenterUsesTechnicalCondition(bool condition)
    {
        var bicycle = new Bicycle("B-1", "Велосипед", 6, condition);
        Assert.Equal(condition, new ServiceCenter().Inspect(bicycle));
    }

    [Fact]
    public void ServiceCenterRejectsNull()
    {
        Assert.Throws<ArgumentNullException>(() => new ServiceCenter().Inspect(null!));
    }
}
