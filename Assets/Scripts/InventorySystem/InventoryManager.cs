using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Inventory
{
    public List<InventoryItem> items = new List<InventoryItem>();
    private IInventoryPolicy policy;

    public Inventory(IInventoryPolicy policy)
    {
        this.policy = policy;
    }

    public void SetPolicy(IInventoryPolicy policy)
    {
        this.policy = policy;
    }

    public void AddItem(ItemData data, int amount)
    {
        if (policy == null)
        {
            Debug.LogError("No policy set!");
            return;
        }
        if (!policy.CanAddItem(this, data, amount))
            return;

        policy.AddItem(this, data, amount);
    }

    public void RemoveItem(ItemData data, int amount)
    {
        var existing = items.Find(i => i.data == data);
        if (existing != null)
        {
            existing.amount -= amount;

            if (existing.amount <= 0)
                items.Remove(existing);
        }
    }
}

public interface IInventoryPolicy
{
    bool CanAddItem(Inventory inventory, ItemData data, int amount);
    void AddItem(Inventory inventory, ItemData data, int amount);
}

public class PlayerInventoryPolicy : IInventoryPolicy
{
    private int maxSlots;

    public PlayerInventoryPolicy(int maxSlots)
    {
        this.maxSlots = maxSlots;
    }

    public bool CanAddItem(Inventory inventory, ItemData data, int amount)
    {
        return inventory.items.Count + amount <= maxSlots;
    }

    public void AddItem(Inventory inventory, ItemData data, int amount)
    {
        for (int i = 0; i < amount; i++)
        {
            if (inventory.items.Count >= maxSlots)
                break;
            inventory.items.Add(new InventoryItem(data, 1));
        }
    }
}

public class StorageInventoryPolicy : IInventoryPolicy
{
    public bool CanAddItem(Inventory inventory, ItemData data, int amount)
    {
        return true;
    }

    public void AddItem(Inventory inventory, ItemData data, int amount)
    {
        var existing = inventory.items.Find(i => i.data == data);

        if (existing != null)
            existing.amount += amount;
        else
            inventory.items.Add(new InventoryItem(data, amount));
    }
}

public class Equipment
{
    private readonly System.Collections.Generic.Dictionary<GearData.GearSlot, GearData> slots =
        new System.Collections.Generic.Dictionary<GearData.GearSlot, GearData>
        {
            { GearData.GearSlot.Head,  null },
            { GearData.GearSlot.Chest, null },
            { GearData.GearSlot.Legs,  null },
            { GearData.GearSlot.Boots, null },
        };

    public GearData Equip(GearData newGear)
    {
        if (newGear == null) return null;

        var slot = newGear.slot;
        var old = slots[slot];

        if (old != null) PlayerStatManager.Instance.ApplyGear(old, -1);
        slots[slot] = newGear;
        PlayerStatManager.Instance.ApplyGear(newGear, +1);

        return old;
    }

    public void Unequip(GearData.GearSlot slot)
    {
        var old = slots[slot];
        if (old == null) return;

        PlayerStatManager.Instance.ApplyGear(old, -1);
        slots[slot] = null;
    }
}


public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }

    public Inventory playerInventory;
    public Inventory storageInventory;

    public InventoryItem SelectedItem { get; private set; }
    public Inventory SourceInventory { get; private set; }

    [Header("Settings")]
    [SerializeField] private int playerSlotLimit = 10;

    [Header("Player Gear")]
    public Equipment playerEquipment { get; private set; }

    [Header("Test items")]
    [SerializeField] private ItemData[] testItems;
    [SerializeField] private int[] testAmounts;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        playerInventory = new Inventory(new PlayerInventoryPolicy(playerSlotLimit));
        storageInventory = new Inventory(new StorageInventoryPolicy());
        playerEquipment = new Equipment();
    }

    private void Start()
    {
        if (testItems != null)
        {
            for (int i = 0; i < testItems.Length; i++)
            {
                int amount = (testAmounts != null && i < testAmounts.Length) ? testAmounts[i] : 1;
                playerInventory.AddItem(testItems[i], amount);
            }
        }
    }

    public void MoveItem(Inventory from, Inventory to, ItemData data, int amount)
    {
        var existing = from.items.Find(i => i.data == data);
        if (existing == null)
            return;

        int moveAmount = Mathf.Min(existing.amount, amount);
        from.RemoveItem(data, moveAmount);
        to.AddItem(data, moveAmount);
    }

    public void SelectItem(InventoryItem item, Inventory source)
    {
        SelectedItem = item;
        SourceInventory = source;
    }

    public void ClearSelection()
    {
        SelectedItem = null;
        SourceInventory = null;
    }
}


