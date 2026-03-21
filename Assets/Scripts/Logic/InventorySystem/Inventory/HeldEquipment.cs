using System;
using System.Collections.Generic;

// This class stores holdable items
[Serializable]
public class HeldEquipment
{
    // Update now we store inventory items, not just static data
    private readonly Dictionary<HeldSlot, InventoryItem> slots = new()
    {
        { HeldSlot.Slot1, null },
        { HeldSlot.Slot2, null }
    };

    public event Action OnChanged;
    public IReadOnlyDictionary<HeldSlot, InventoryItem> Slots => slots;

    // === Getters ===
    public InventoryItem GetEquippedItem(HeldSlot slot) // Getting inventory item from slot
    {
        slots.TryGetValue(slot, out var item);
        return item;
    }

    // === Setters ===
    public InventoryItem Equip(InventoryItem item, HeldSlot slot) // Equipping inventoryItem to slot and passing out old item
    {
        InventoryItem old = GetEquippedItem(slot);
        slots[slot] = item;
        OnChanged?.Invoke();
        return old;
    }
    public InventoryItem Unequip(HeldSlot slot) // Unequipping item
    {
        InventoryItem old = GetEquippedItem(slot);
        slots[slot] = null;
        OnChanged?.Invoke();
        return old;
    }

    // === Queries ===
    public bool Contains(InventoryItem item) // Checking for item in slots
    {
        if (item == null)
            return false;

        foreach (var kv in slots)
        {
            if (ReferenceEquals(kv.Value, item))
                return true;
        }

        return false;
    }
    public bool TryFindSlot(InventoryItem item, out HeldSlot slot) // Finding slot for item
    {
        foreach (var kv in slots)
        {
            if (ReferenceEquals(kv.Value, item))
            {
                slot = kv.Key;
                return true;
            }
        }

        slot = default;
        return false;
    }
    public HeldSlot? GetFirstEmptySlot()
    {
        foreach (var kv in slots)
        {
            if (kv.Value == null)
                return kv.Key;
        }

        return null;
    }

    // === Helpers ===
    public void Clear()
    {
        slots[HeldSlot.Slot1] = null;
        slots[HeldSlot.Slot2] = null;
        OnChanged?.Invoke();
    }
}

public enum HeldSlot
{
    Slot1 = 0,
    Slot2 = 1
}