namespace VyshKat.Core.Model;

public abstract class InventoryItem : IInventoryItem
{
    protected InventoryItem(string inventoryNumber, string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(inventoryNumber);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        InventoryNumber = inventoryNumber.Trim();
        Name = name.Trim();
    }

    public string InventoryNumber { get; }
    public string Name { get; }
}
