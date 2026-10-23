namespace VyshKat.Core.Model;

public abstract class Equipment : InventoryItem
{
    protected Equipment(string inventoryNumber, string name) : base(inventoryNumber, name)
    {
    }
}
