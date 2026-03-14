using System;
using System.Collections.Generic;

[Serializable]
public class HeldEquipment
{
    private readonly Dictionary<HeldSlot, HoldableItemData> _slots = new()
    {
        { HeldSlot.Slot1, null },
        { HeldSlot.Slot2, null }
    };

    public event Action OnChanged;

    public IReadOnlyDictionary<HeldSlot, HoldableItemData> Slots => _slots;

    public HoldableItemData GetEquipped(HeldSlot slot)
    {
        _slots.TryGetValue(slot, out var item);
        return item;
    }

    public HoldableItemData Equip(HoldableItemData item, HeldSlot slot)
    {
        HoldableItemData old = GetEquipped(slot);
        _slots[slot] = item;
        OnChanged?.Invoke();
        return old;
    }

    public HoldableItemData Unequip(HeldSlot slot)
    {
        HoldableItemData old = GetEquipped(slot);
        _slots[slot] = null;
        OnChanged?.Invoke();
        return old;
    }

    public bool Contains(HoldableItemData item)
    {
        if (item == null) return false;

        foreach (var kv in _slots)
        {
            if (ReferenceEquals(kv.Value, item))
                return true;
        }

        return false;
    }

    public bool TryFindSlot(HoldableItemData item, out HeldSlot slot)
    {
        foreach (var kv in _slots)
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
        foreach (var kv in _slots)
        {
            if (kv.Value == null)
                return kv.Key;
        }

        return null;
    }

    public void Clear()
    {
        _slots[HeldSlot.Slot1] = null;
        _slots[HeldSlot.Slot2] = null;
        OnChanged?.Invoke();
    }
}

public enum HeldSlot
{
    Slot1 = 0,
    Slot2 = 1
}