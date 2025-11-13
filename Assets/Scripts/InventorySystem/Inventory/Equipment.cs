using System;

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