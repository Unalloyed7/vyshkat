namespace VyshKat.Core.Model;

public sealed class Bicycle : Transport
{
    public Bicycle(string inventoryNumber, string name, int beginnerEase, bool isTechnicallySound)
        : base(inventoryNumber, name, beginnerEase, isTechnicallySound)
    {
    }
}
