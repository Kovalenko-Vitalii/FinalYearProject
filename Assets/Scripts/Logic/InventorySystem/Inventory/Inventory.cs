using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class Inventory
{
    public List<InventoryItem> items = new List<InventoryItem>();
    private IInventoryPolicy policy;

    public int maxSlots => policy?.MaxSlots ?? -1;
    public int currentSlots => items?.Count ?? 0;

    public event Action OnChanged;

    public Inventory(IInventoryPolicy policy) { this.policy = policy; }
    public void SetPolicy(IInventoryPolicy policy) { this.policy = policy; }

    // === Adding ===
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
    public bool TryAddItemInstance(InventoryItem item)
    {
        if (item == null || item.data == null || item.amount <= 0)
            return false;

        if (policy == null)
            return false;

        item.EnsureRuntimeState();
        item.EnsureInstanceId();

        if (item.MustRemainUniqueInstance)
        {
            if (item.amount != 1)
            {
                Debug.LogError($"[Inventory] Unique item '{item.data.id}' must have amount = 1.");
                return false;
            }

            if (!policy.CanAddItem(this, item.data, 1))
                return false;

            items.Add(item);
            OnChanged?.Invoke();
            return true;
        }

        int accepted = AddItemAndGetAccepted(item.data, item.amount, item.currentDurability);
        return accepted == item.amount;
    }

    // === Removing ===
    public void RemoveItem(ItemData data, int amount)
    {
        if (data == null || amount <= 0) return;

        for (int i = items.Count - 1; i >= 0 && amount > 0; i--)
        {
            var stack = items[i];
            if (stack.data != data) continue;

            int take = Mathf.Min(amount, stack.amount);
            stack.amount -= take;
            amount -= take;

            if (stack.amount <= 0)
                items.RemoveAt(i);
        }

        CompactStacks(data);
        OnChanged?.Invoke();
    }
    public bool RemoveItemInstance(InventoryItem item)
    {
        if (item == null)
            return false;

        bool removed = items.Remove(item);
        if (removed)
            OnChanged?.Invoke();

        return removed;
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

    // === Queries ===
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
    public InventoryItem GetInventoryItemById(string id)
    {
        foreach (var item in items)
        {
            if (item.data.id == id)
            {
                return item;
            }
        }
        return null;
    }
    public InventoryItem GetItemByInstanceId(string instanceId)
    {
        if (string.IsNullOrWhiteSpace(instanceId))
            return null;

        foreach (var item in items)
        {
            if (item != null && item.instanceId == instanceId)
                return item;
        }

        return null;
    }
    public int GetTotalAmountById(string id)
    {
        int total = 0;
        foreach (var item in items)
        {
            if (item?.data != null && item.data.id == id)
                total += item.amount;
        }
        return total;
    }

    // === Helpers ===
    private void CompactStacks(ItemData data)
    {
        if (!InventoryRules.IsStackable(data))
            return;

        int max = data.maxStack;

        for (int i = 0; i < items.Count; i++)
        {
            var a = items[i];
            if (a.data != data || a.amount >= max)
                continue;

            for (int j = i + 1; j < items.Count && a.amount < max; j++)
            {
                var b = items[j];
                if (b.data != data)
                    continue;

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
}


public interface IInventoryPolicy
{
    int MaxSlots { get; }

    bool CanAddItem(Inventory inventory, ItemData data, int amount);
    void AddItem(Inventory inventory, ItemData data, int amount, float durability);
}

public class PlayerInventoryPolicy : IInventoryPolicy
{
    private readonly int maxSlots;
    public int MaxSlots => maxSlots;
    public PlayerInventoryPolicy(int maxSlots)
    {
        this.maxSlots = maxSlots;
    }

    public bool CanAddItem(Inventory inventory, ItemData data, int amount)
    {
        return GetAcceptableAmount(inventory, data, amount) >= amount;
    }

    public void AddItem(Inventory inventory, ItemData data, int amount, float currentDurability)
    {
        if (data == null || amount <= 0)
            return;

        if (!CanAddItem(inventory, data, amount))
            return;

        if (!InventoryRules.IsStackable(data))
        {
            while (amount > 0 && inventory.items.Count < maxSlots)
            {
                inventory.items.Add(new InventoryItem(data, 1, currentDurability));
                amount--;
            }

            return;
        }

        foreach (var stack in inventory.items)
        {
            if (stack.data != data || stack.amount >= data.maxStack)
                continue;

            int put = Mathf.Min(amount, data.maxStack - stack.amount);
            stack.amount += put;
            amount -= put;

            if (amount <= 0)
                return;
        }

        while (amount > 0 && inventory.items.Count < maxSlots)
        {
            int put = Mathf.Min(amount, data.maxStack);
            inventory.items.Add(new InventoryItem(data, put, currentDurability));
            amount -= put;
        }
    }

    private int GetAcceptableAmount(Inventory inventory, ItemData data, int requestedAmount)
    {
        if (inventory == null || data == null || requestedAmount <= 0)
            return 0;

        if (!InventoryRules.IsStackable(data))
        {
            int freeSlots = Mathf.Max(0, maxSlots - inventory.items.Count);
            return Mathf.Min(requestedAmount, freeSlots);
        }

        int accepted = 0;

        foreach (var stack in inventory.items)
        {
            if (stack == null || stack.data != data)
                continue;

            accepted += Mathf.Max(0, data.maxStack - stack.amount);

            if (accepted >= requestedAmount)
                return requestedAmount;
        }

        int freeSlotCount = Mathf.Max(0, maxSlots - inventory.items.Count);
        accepted += freeSlotCount * data.maxStack;

        return Mathf.Min(accepted, requestedAmount);
    }
}

public class StorageInventoryPolicy : IInventoryPolicy
{
    private readonly int maxSlots;
    public int MaxSlots => maxSlots;
    private readonly HashSet<string> allowTags;

    public StorageInventoryPolicy(int maxSlots = -1, IEnumerable<string> allowTags = null)
    {
        this.maxSlots = maxSlots;
        this.allowTags = allowTags != null ? new HashSet<string>(allowTags) : null;
    }

    public bool CanAddItem(Inventory inventory, ItemData data, int amount)
    {
        if (inventory == null || data == null || amount <= 0)
            return false;

        if (allowTags != null && data is ITagsProvider tp)
        {
            bool ok = tp.Tags.Any(tag => allowTags.Contains(tag));
            if (!ok)
                return false;
        }

        if (maxSlots < 0)
            return true;

        if (!InventoryRules.IsStackable(data))
        {
            int freeSlots = Mathf.Max(0, maxSlots - inventory.items.Count);
            return freeSlots >= amount;
        }

        int accepted = 0;

        foreach (var stack in inventory.items)
        {
            if (stack == null || stack.data != data)
                continue;

            accepted += Mathf.Max(0, data.maxStack - stack.amount);

            if (accepted >= amount)
                return true;
        }

        int freeSlotCount = Mathf.Max(0, maxSlots - inventory.items.Count);
        accepted += freeSlotCount * data.maxStack;

        return accepted >= amount;
    }

    public void AddItem(Inventory inventory, ItemData data, int amount, float currentDurability)
    {
        if (inventory == null || data == null || amount <= 0)
            return;

        if (!CanAddItem(inventory, data, amount))
            return;

        if (!InventoryRules.IsStackable(data))
        {
            while (amount > 0 && (maxSlots < 0 || inventory.items.Count < maxSlots))
            {
                inventory.items.Add(new InventoryItem(data, 1, currentDurability));
                amount--;
            }

            return;
        }

        foreach (var stack in inventory.items)
        {
            if (stack.data != data || stack.amount >= data.maxStack)
                continue;

            int put = Mathf.Min(amount, data.maxStack - stack.amount);
            stack.amount += put;
            amount -= put;

            if (amount <= 0)
                return;
        }

        while (amount > 0 && (maxSlots < 0 || inventory.items.Count < maxSlots))
        {
            int put = Mathf.Min(amount, data.maxStack);
            inventory.items.Add(new InventoryItem(data, put, currentDurability));
            amount -= put;
        }
    }
}

public interface ITagsProvider
{
    IEnumerable<string> Tags { get; }
}

public static class InventoryRules
{
    public static bool IsStackable(ItemData data)
    {
        if (data == null)
            return false;

        if (data.maxStack <= 1)
            return false;

        if (data.hasDurability)
            return false;

        if (data is IEquippableItemData)
            return false;

        return true;
    }
}