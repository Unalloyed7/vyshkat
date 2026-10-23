namespace VyshKat.Core.Model;

public sealed class Scooter : ElectricTransport
{
    public Scooter(
        string inventoryNumber, string name, int beginnerEase, bool isTechnicallySound, decimal dailyEnergyKwh)
        : base(inventoryNumber, name, beginnerEase, isTechnicallySound, dailyEnergyKwh)
    {
    }
}
