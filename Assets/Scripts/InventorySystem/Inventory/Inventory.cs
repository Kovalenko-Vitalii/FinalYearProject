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

    public void AddItem(ItemData data, int amount, float currentDurability)
    {
        if (policy == null) { Debug.LogError("No policy set!"); return; }
        if (!policy.CanAddItem(this, data, amount)) return;

        policy.AddItem(this, data, amount, currentDurability);

        CompactStacks(data);
        OnChanged?.Invoke();
    }

    public bool CanAdd(ItemData data, int amount)
    {
        if (policy == null || data == null || amount <= 0)
            return false;

        return policy.CanAddItem(this, data, amount);
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

    public int AddItemAndGetAccepted(ItemData data, int amount, float currentDurability)
    {
        if (policy == null || data == null || amount <= 0) return 0;

        int before = items.Where(i => i.data == data).Sum(i => i.amount);
        int countBefore = items.Count;

        if (!policy.CanAddItem(this, data, amount))
            return 0;

        policy.AddItem(this, data, amount, currentDurability);

        CompactStacks(data);
        OnChanged?.Invoke();

        int after = items.Where(i => i.data == data).Sum(i => i.amount);
        return Mathf.Clamp(after - before, 0, amount);
    }

    public bool HasItemById(string id)
    {
        foreach (var item in items)
        {
            if (item.data.id == id)
            {
                return true;
            }
        }
        return false;
    }
}


public interface IInventoryPolicy
{
    bool CanAddItem(Inventory inventory, ItemData data, int amount);
    void AddItem(Inventory inventory, ItemData data, int amount, float durability);
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

    public void AddItem(Inventory inventory, ItemData data, int amount, float currentDurability)
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
                inventory.items.Add(new InventoryItem(data, put, currentDurability));
                left -= put;
            }
        }
        else
        {
            while (amount > 0 && inventory.items.Count < maxSlots)
            {
                int put = Mathf.Min(amount, data.maxStack);
                inventory.items.Add(new InventoryItem(data, put, currentDurability));
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

    public void AddItem(Inventory inventory, ItemData data, int amount, float currentDurability)
    {
        var existing = inventory.items.Find(i => i.data == data);
        if (existing != null)
        {
            existing.amount += amount;
        }
        else
        {
            inventory.items.Add(new InventoryItem(data, amount, currentDurability));
        }
    }

}

public interface ITagsProvider
{
    IEnumerable<string> Tags { get; }
}
 