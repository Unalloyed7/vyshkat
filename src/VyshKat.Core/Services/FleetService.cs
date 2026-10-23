using VyshKat.Core.Model;

namespace VyshKat.Core.Services;

public sealed class FleetService
{
    private readonly IServiceCenter serviceCenter;
    private readonly Dictionary<string, IInventoryItem> inventory =
        new Dictionary<string, IInventoryItem>(StringComparer.OrdinalIgnoreCase);

    public FleetService(IServiceCenter serviceCenter)
    {
        ArgumentNullException.ThrowIfNull(serviceCenter);
        this.serviceCenter = serviceCenter;
    }

    public AcceptanceResult AddTransport(Transport transport)
    {
        ArgumentNullException.ThrowIfNull(transport);
        if (inventory.ContainsKey(transport.InventoryNumber))
        {
            return AcceptanceResult.DuplicateInventoryNumber;
        }

        if (!serviceCenter.Inspect(transport))
        {
            return AcceptanceResult.RejectedByInspection;
        }

        inventory.Add(transport.InventoryNumber, transport);
        return AcceptanceResult.Accepted;
    }

    public AcceptanceResult AddEquipment(Equipment equipment)
    {
        ArgumentNullException.ThrowIfNull(equipment);
        if (inventory.ContainsKey(equipment.InventoryNumber))
        {
            return AcceptanceResult.DuplicateInventoryNumber;
        }

        inventory.Add(equipment.InventoryNumber, equipment);
        return AcceptanceResult.Accepted;
    }

    public int TransportCount
    {
        get
        {
            int count = 0;
            foreach (IInventoryItem item in inventory.Values)
            {
                if (item is Transport)
                {
                    count++;
                }
            }

            return count;
        }
    }

    public int EquipmentCount
    {
        get { return inventory.Count - TransportCount; }
    }

    public decimal TotalDailyEnergyKwh
    {
        get
        {
            decimal total = 0;
            foreach (IInventoryItem item in inventory.Values)
            {
                if (item is IEnergyConsumer consumer)
                {
                    total += consumer.DailyEnergyKwh;
                }
            }

            return total;
        }
    }

    public IReadOnlyList<Transport> GetBeginnerTransport()
    {
        List<Transport> result = new List<Transport>();
        foreach (IInventoryItem item in inventory.Values)
        {
            if (item is Transport transport && transport.IsSuitableForBeginners)
            {
                result.Add(transport);
            }
        }

        return result.ToArray();
    }

    public IReadOnlyList<IInventoryItem> GetInventory()
    {
        IInventoryItem[] items = new IInventoryItem[inventory.Count];
        inventory.Values.CopyTo(items, 0);
        return items;
    }
}
