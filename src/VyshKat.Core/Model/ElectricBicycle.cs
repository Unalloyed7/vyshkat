namespace VyshKat.Core.Model;

public sealed class ElectricBicycle : ElectricTransport
{
    public ElectricBicycle(
        string inventoryNumber, string name, int beginnerEase, bool isTechnicallySound, decimal dailyEnergyKwh)
        : base(inventoryNumber, name, beginnerEase, isTechnicallySound, dailyEnergyKwh)
    {
    }
}
