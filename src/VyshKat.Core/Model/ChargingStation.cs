namespace VyshKat.Core.Model;

public sealed class ChargingStation : Equipment, IEnergyConsumer
{
    public ChargingStation(string inventoryNumber, string name, decimal dailyEnergyKwh)
        : base(inventoryNumber, name)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(dailyEnergyKwh);
        DailyEnergyKwh = dailyEnergyKwh;
    }

    public decimal DailyEnergyKwh { get; }
}
