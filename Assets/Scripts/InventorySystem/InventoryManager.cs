using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class Inventory
{
    public List<InventoryItem> items = new List<InventoryItem>();
    private IInventoryPolicy policy;

    public event Action OnChanged;

    public Inventory(IInventoryPolicy policy) { this.policy = policy; }

    public void SetPolicy(IInventoryPolicy policy) { this.policy = policy; }

    public void AddItem(ItemData data, int amount)
    {
        if (policy == null) { Debug.LogError("No policy set!"); return; }
        if (!policy.CanAddItem(this, data, amount)) return;

        policy.AddItem(this, data, amount);

        CompactStacks(data);
        OnChanged?.Invoke();
    }

    public void RemoveItem(ItemData data, int amount)
    {
        var existing = items.Find(i => i.data == data);
        if (existing != null)
            RemoveFromStack(existing, amount);
    }

    public void RemoveFromStack(InventoryItem stack, int amount)
    {
        int idx = items.IndexOf(stack);
        if (idx < 0) return;

        int remove = Mathf.Min(amount, stack.amount);
        stack.amount -= remove;

        if (stack.amount <= 0)
            items.RemoveAt(idx);

        CompactStacks(stack.data);

        OnChanged?.Invoke();
    }

    private void CompactStacks(ItemData data)
    {
        if (data == null) return;
        int max = data.maxStack;

        for (int i = 0; i < items.Count; i++)
        {
            var a = items[i];
            if (a.data != data || a.amount >= max) continue;

            for (int j = i + 1; j < items.Count && a.amount < max; j++)
            {
                var b = items[j];
                if (b.data != data) continue;

                int transfer = Mathf.Min(max - a.amount, b.amount);
                a.amount += transfer;
                b.amount -= transfer;

                if (b.amount <= 0)
                {
                    items.RemoveAt(j);
                    j--;
                }
            }
        }
    }

    public int AddItemAndGetAccepted(ItemData data, int amount)
    {
        if (policy == null || data == null || amount <= 0) return 0;

        int before = items.Where(i => i.data == data).Sum(i => i.amount);
        int countBefore = items.Count;

        if (!policy.CanAddItem(this, data, amount))
            return 0;

        policy.AddItem(this, data, amount);

        CompactStacks(data);
        OnChanged?.Invoke();

        int after = items.Where(i => i.data == data).Sum(i => i.amount);
        return Mathf.Clamp(after - before, 0, amount);
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
        var existing = inventory.items.Find(i => i.data == data);
        if (existing != null)
        {
            return existing.amount + amount <= data.maxStack
                   || inventory.items.Count < maxSlots;
        }
        else
        {
            return inventory.items.Count + 1 <= maxSlots;
        }
    }

    public void AddItem(Inventory inventory, ItemData data, int amount)
    {
        var existing = inventory.items.Find(i => i.data == data);
        if (existing != null)
        {
            int canPut = Mathf.Min(amount, data.maxStack - existing.amount);
            existing.amount += canPut;
            int left = amount - canPut;

            while (left > 0 && inventory.items.Count < maxSlots)
            {
                int put = Mathf.Min(left, data.maxStack);
                inventory.items.Add(new InventoryItem(data, put));
                left -= put;
            }
        }
        else
        {
            while (amount > 0 && inventory.items.Count < maxSlots)
            {
                int put = Mathf.Min(amount, data.maxStack);
                inventory.items.Add(new InventoryItem(data, put));
                amount -= put;
            }
        }
    }
}

public class StorageInventoryPolicy : IInventoryPolicy
{
    private readonly int maxSlots;
    private readonly HashSet<string> allowTags;

    public StorageInventoryPolicy(int maxSlots = -1, IEnumerable<string> allowTags = null)
    {
        this.maxSlots = maxSlots;
        this.allowTags = allowTags != null ? new HashSet<string>(allowTags) : null;
    }

    public bool CanAddItem(Inventory inventory, ItemData data, int amount)
    {
        if (allowTags != null && data is ITagsProvider tp)
        {
            bool ok = tp.Tags.Any(t => allowTags.Contains(t));
            if (!ok) return false;
        }

        if (maxSlots < 0) return true;

        var existing = inventory.items.Find(i => i.data == data);
        if (existing != null)
        {
            return existing.amount + amount <= data.maxStack
                   || inventory.items.Count < maxSlots;
        }
        else
        {
            return inventory.items.Count + 1 <= maxSlots;
        }
    }

    public void AddItem(Inventory inventory, ItemData data, int amount)
    {
        var existing = inventory.items.Find(i => i.data == data);
        if (existing != null)
        {
            existing.amount += amount;
        }
        else
        {
            inventory.items.Add(new InventoryItem(data, amount));
        }
    }
}

public interface ITagsProvider
{
    IEnumerable<string> Tags { get; }
}


public class Equipment
{
    public event Action<GearData.GearSlot, GearData, GearData> OnChanged;

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

        OnChanged?.Invoke(slot, old, newGear);
        return old;
    }

    public void Unequip(GearData.GearSlot slot)
    {
        var old = slots[slot];
        if (old == null) return;

        PlayerStatManager.Instance.ApplyGear(old, -1);
        slots[slot] = null;

        OnChanged?.Invoke(slot, old, null);
    }

    public GearData GetEquipped(GearData.GearSlot slot)
    {
        if (slots.TryGetValue(slot, out var equipped))
            return equipped;
        return null;
    }

}


public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }

    public Inventory playerInventory;

    public InventoryItem SelectedItem { get; private set; }
    public Inventory SourceInventory { get; private set; }

    [Header("Settings")]
    [SerializeField] private int playerSlotLimit = 10;

    [Header("Player Gear")]
    public Equipment playerEquipment { get; private set; }

    [Header("Test items")]
    [SerializeField] private ItemData[] testItems;
    [SerializeField] private int[] testAmounts;

    public event Action OnPlayerInventoryChanged;
    public readonly List<WorldContainer> containers = new();

    public Transform playerDropOrigin;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        playerInventory = new Inventory(new PlayerInventoryPolicy(playerSlotLimit));
        playerEquipment = new Equipment();

        playerInventory.OnChanged += () => OnPlayerInventoryChanged?.Invoke();
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
        if (from == null || to == null || data == null || amount <= 0) return;

        var src = from.items.Find(i => i.data == data);
        if (src == null || src.amount <= 0) return;

        int canMove = Mathf.Min(src.amount, amount);

        int beforeToCount = to.items.Where(i => i.data == data).Sum(i => i.amount);
        to.AddItem(data, canMove);
        int afterToCount = to.items.Where(i => i.data == data).Sum(i => i.amount);

        int accepted = Mathf.Clamp(afterToCount - beforeToCount, 0, canMove);
        if (accepted <= 0) return;

        from.RemoveItem(data, accepted);
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

    public void Register(WorldContainer c)
    {
        if (c != null && !containers.Contains(c)) containers.Add(c);
    }

    public void Unregister(WorldContainer c)
    {
        containers.Remove(c);
    }

    public Inventory GetContainerInventory(WorldContainer c) => c?.Inventory;

}


