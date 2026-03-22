using System;
using System.Collections.Generic;
using System.Linq;

public class EquippedItems
{
    private readonly Dictionary<EquipmentSlotId, InventoryItem> slots;

    public event Action<EquipmentSlotId, InventoryItem, InventoryItem> OnSlotChanged;

    public EquippedItems(IEnumerable<EquipmentSlotId> availableSlots)
    {
        slots = availableSlots.ToDictionary(slot => slot, _ => (InventoryItem)null);
    }

    public IReadOnlyDictionary<EquipmentSlotId, InventoryItem> Slots => slots;

    public bool HasSlot(EquipmentSlotId slot)
    {
        return slots.ContainsKey(slot);
    }

    public InventoryItem Get(EquipmentSlotId slot)
    {
        return slots.TryGetValue(slot, out var item) ? item : null;
    }

    public bool Contains(InventoryItem item)
    {
        return item != null && slots.Values.Any(x => ReferenceEquals(x, item));
    }

    public bool TryFindSlot(InventoryItem item, out EquipmentSlotId slot)
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

    public EquipmentSlotId? GetFirstEmpty(params EquipmentSlotId[] candidates)
    {
        var source = candidates != null && candidates.Length > 0
            ? candidates
            : slots.Keys.ToArray();

        foreach (var slot in source)
        {
            if (Get(slot) == null)
                return slot;
        }

        return null;
    }

    public InventoryItem Equip(EquipmentSlotId slot, InventoryItem item)
    {
        if (!slots.ContainsKey(slot))
            throw new ArgumentOutOfRangeException(nameof(slot), $"Slot '{slot}' is not registered.");

        var old = slots[slot];

        if (ReferenceEquals(old, item))
            return old;

        slots[slot] = item;
        OnSlotChanged?.Invoke(slot, old, item);
        return old;
    }

    public InventoryItem Unequip(EquipmentSlotId slot)
    {
        if (!slots.ContainsKey(slot))
            throw new ArgumentOutOfRangeException(nameof(slot), $"Slot '{slot}' is not registered.");

        var old = slots[slot];
        if (old == null)
            return null;

        slots[slot] = null;
        OnSlotChanged?.Invoke(slot, old, null);
        return old;
    }

    public void Clear()
    {
        foreach (var slot in slots.Keys.ToList())
        {
            if (slots[slot] != null)
                Unequip(slot);
        }
    }
}

public enum EquipmentSlotId
{
    Head = 0,
    Chest = 1,
    Legs = 2,
    Boots = 3,
    Held1 = 4,
    Held2 = 5
}

public static class EquipmentSlots
{
    public static readonly EquipmentSlotId[] All =
    {
        EquipmentSlotId.Head,
        EquipmentSlotId.Chest,
        EquipmentSlotId.Legs,
        EquipmentSlotId.Boots,
        EquipmentSlotId.Held1,
        EquipmentSlotId.Held2
    };

    public static readonly EquipmentSlotId[] Held =
    {
        EquipmentSlotId.Held1,
        EquipmentSlotId.Held2
    };

    public static bool IsHeld(EquipmentSlotId slot)
    {
        return slot == EquipmentSlotId.Held1 || slot == EquipmentSlotId.Held2;
    }
}

public interface IEquippableItemData
{
    IReadOnlyList<EquipmentSlotId> AllowedSlots { get; }
}