namespace VyshKat.Core.Model;

public abstract class ElectricTransport : Transport, IEnergyConsumer
{
    protected ElectricTransport(
        string inventoryNumber, string name, int beginnerEase, bool isTechnicallySound, decimal dailyEnergyKwh)
        : base(inventoryNumber, name, beginnerEase, isTechnicallySound)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(dailyEnergyKwh);
        DailyEnergyKwh = dailyEnergyKwh;
    }

    public decimal DailyEnergyKwh { get; }
}
