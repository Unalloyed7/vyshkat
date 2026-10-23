namespace VyshKat.Core.Model;

public abstract class Transport : InventoryItem
{
    protected Transport(string inventoryNumber, string name, int beginnerEase, bool isTechnicallySound)
        : base(inventoryNumber, name)
    {
        if (beginnerEase < 1 || beginnerEase > 10)
        {
            throw new ArgumentOutOfRangeException(nameof(beginnerEase), "Простота должна быть от 1 до 10.");
        }

        BeginnerEase = beginnerEase;
        IsTechnicallySound = isTechnicallySound;
    }

    public int BeginnerEase { get; }
    public bool IsTechnicallySound { get; }
    public bool IsSuitableForBeginners
    {
        get { return BeginnerEase >= 6; }
    }
}
