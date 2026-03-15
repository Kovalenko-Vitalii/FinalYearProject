using System;
using System.Collections.Generic;

// This class stores holdable items
[Serializable]
public class HeldEquipment
{
    private readonly Dictionary<HeldSlot, HoldableItemData> slots = new()
    {
        { HeldSlot.Slot1, null },
        { HeldSlot.Slot2, null }
    };

    public event Action OnChanged;

    public IReadOnlyDictionary<HeldSlot, HoldableItemData> Slots => slots;

    // Getting equipped slot
    public HoldableItemData GetEquipped(HeldSlot slot)
    {
        slots.TryGetValue(slot, out var item);
        return item;
    }

    // Equipping holdable to slot and passing out old item
    public HoldableItemData Equip(HoldableItemData item, HeldSlot slot)
    {
        HoldableItemData old = GetEquipped(slot);
        slots[slot] = item;
        OnChanged?.Invoke();
        return old;
    }

    // Unequipping
    public HoldableItemData Unequip(HeldSlot slot)
    {
        HoldableItemData old = GetEquipped(slot);
        slots[slot] = null;
        OnChanged?.Invoke();
        return old;
    }

    // Checking for item in slots
    public bool Contains(HoldableItemData item)
    {
        if (item == null) return false;

        foreach (var kv in slots)
        {
            if (ReferenceEquals(kv.Value, item))
                return true;
        }

        return false;
    }

    // Finding slot for item
    public bool TryFindSlot(HoldableItemData item, out HeldSlot slot)
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