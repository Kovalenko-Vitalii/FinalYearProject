using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class InventoryManager : MonoBehaviour, ISaveable
{
    public static InventoryManager Instance { get; private set; }

    public Inventory playerInventory;

    public InventoryItem SelectedItem { get; private set; }
    public Inventory SourceInventory { get; private set; }


    [Header("Settings")]
    [SerializeField] private int playerSlotLimit = 10;

    // ====================================================

    [Header("Player Gear")]
    public Equipment playerEquipment { get; private set; }

    [Header("Player Held Slots")]
    public HeldEquipment playerHeldEquipment { get; private set; }

    public HeldSlot? ActiveHeldSlot { get; private set; }

    public event Action OnHeldEquipmentChanged;
    public event Action<HeldSlot?> OnActiveHeldSlotChanged;
    
    // =====================================================

    [Header("Test items")]
    [SerializeField] private ItemData[] initialItems;
    [SerializeField] private int[] initialAmounts;


    public event Action OnPlayerInventoryChanged;

    public event Action<InventoryItem, Inventory> OnSelectionChanged;

    public string EquippedHeldItemId { get; private set; }
    public event Action<string> OnEquippedHeldItemChanged;

    public string SaveId => "PLAYER_INVENTORY";

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

        playerHeldEquipment = new HeldEquipment();
        playerHeldEquipment.OnChanged += () => OnHeldEquipmentChanged?.Invoke();
    }

    private void Start()
    {
        if (initialItems != null)
        {
            for (int i = 0; i < initialItems.Length; i++)
            {
                int amount = (initialAmounts != null && i < initialAmounts.Length) ? initialAmounts[i] : 1;
                if (initialItems[i] is GearData gearData)
                {
                    playerEquipment.Equip(gearData);
                    continue;
                }

                playerInventory.AddItem(initialItems[i], amount, initialItems[i].maxDurability);
            }
        }
    }

    public void MoveItem(Inventory from, Inventory to, ItemData data, int amount, float currentDurability)
    {
        if (from == null || to == null || data == null || amount <= 0) return;

        var src = from.items.Find(i => i.data == data);
        if (src == null || src.amount <= 0) return;

        int canMove = Mathf.Min(src.amount, amount);

        int beforeToCount = to.items.Where(i => i.data == data).Sum(i => i.amount);
        to.AddItem(data, canMove, currentDurability);
        int afterToCount = to.items.Where(i => i.data == data).Sum(i => i.amount);

        int accepted = Mathf.Clamp(afterToCount - beforeToCount, 0, canMove);
        if (accepted <= 0) return;

        from.RemoveItem(data, accepted);
    }

    public void SelectItem(InventoryItem item, Inventory source)
    {
        SelectedItem = item;
        SourceInventory = source;
        OnSelectionChanged?.Invoke(item, source);
    }

    public void ClearSelection()
    {
        SelectedItem = null;
        SourceInventory = null;
        OnSelectionChanged?.Invoke(null, null);
    }

    public bool TryToggleEquipHeldItem(InventoryItem inventoryItem)
    {
        if (inventoryItem == null || inventoryItem.data is not HoldableItemData holdable)
            return false;

        if (playerHeldEquipment.TryFindSlot(holdable, out HeldSlot existingSlot))
        {
            playerHeldEquipment.Unequip(existingSlot);

            if (ActiveHeldSlot == existingSlot)
                SetActiveHeldSlot(null);

            OnPlayerInventoryChanged?.Invoke();
            return true;
        }

        HeldSlot targetSlot;
        var empty = playerHeldEquipment.GetFirstEmptySlot();
        if (empty.HasValue)
        {
            targetSlot = empty.Value;
        }
        else
        {
            targetSlot = ActiveHeldSlot ?? HeldSlot.Slot1;
        }

        playerHeldEquipment.Equip(holdable, targetSlot);
        OnPlayerInventoryChanged?.Invoke();
        return true;
    }

    // === Holdable ===
    public void ToggleActiveHeldSlot(HeldSlot slot)
    {
        var item = playerHeldEquipment.GetEquipped(slot);

        if (item == null)
            return;

        if (ActiveHeldSlot == slot)
        {
            SetActiveHeldSlot(null);
            return;
        }

        SetActiveHeldSlot(slot);
    }

    public void SetActiveHeldSlot(HeldSlot? slot)
    {
        if (ActiveHeldSlot == slot)
            return;

        ActiveHeldSlot = slot;
        OnActiveHeldSlotChanged?.Invoke(slot);
    }

    public HoldableItemData GetActiveHeldItemData()
    {
        if (!ActiveHeldSlot.HasValue)
            return null;

        return playerHeldEquipment.GetEquipped(ActiveHeldSlot.Value);
    }

    // === Save / Load ===
    public object CaptureState()
    {
        var data = new SaveInventoryData();

        // inventory
        foreach (var it in playerInventory.items)
        {
            if (it == null || it.data == null) continue;
            if (it.amount <= 0) continue;

            data.inventoryItems.Add(new InventoryItemSave
            {
                itemId = it.data.id,
                amount = it.amount,
                durability = it.currentDurability
            });
        }

        // gear
        foreach (var kv in playerEquipment.slots)
        {
            var gear = kv.Value;
            if (gear == null) continue;

            data.gearSlots.Add(new GearPairSave
            {
                slot = kv.Key,
                gearId = gear.id,
                durability = 0f
            });
        }

        var held1 = playerHeldEquipment.GetEquipped(HeldSlot.Slot1);
        var held2 = playerHeldEquipment.GetEquipped(HeldSlot.Slot2);

        data.heldSlot1ItemId = held1 != null ? held1.id : null;
        data.heldSlot2ItemId = held2 != null ? held2.id : null;
        data.activeHeldSlot = ActiveHeldSlot.HasValue ? (int)ActiveHeldSlot.Value : -1;

        return data;
    }

    public void RestoreState(object state)
    {
        var data = state as SaveInventoryData;

        playerInventory.items.Clear();
        playerHeldEquipment.Clear();
        SetActiveHeldSlot(null);

        playerEquipment.Unequip(GearData.GearSlot.Head);
        playerEquipment.Unequip(GearData.GearSlot.Chest);
        playerEquipment.Unequip(GearData.GearSlot.Legs);
        playerEquipment.Unequip(GearData.GearSlot.Boots);

        if (data == null)
        {
            OnPlayerInventoryChanged?.Invoke();
            ClearSelection();
            return;
        }

        if (data.inventoryItems != null)
        {
            foreach (var s in data.inventoryItems)
            {
                var itemData = ItemResolver.Resolve(s.itemId);
                if (itemData == null)
                {
                    Debug.LogWarning($"[Inventory] Unknown itemId '{s.itemId}'");
                    continue;
                }

                playerInventory.AddItem(itemData, s.amount, s.durability);
            }
        }

        if (data.gearSlots != null)
        {
            foreach (var p in data.gearSlots)
            {
                if (string.IsNullOrWhiteSpace(p.gearId)) continue;

                var item = ItemResolver.Resolve(p.gearId);
                if (item == null)
                {
                    Debug.LogWarning($"[Inventory] Unknown gearId '{p.gearId}'");
                    continue;
                }

                if (item is not GearData gear)
                {
                    Debug.LogWarning($"[Inventory] itemId '{p.gearId}' is not GearData (got {item.GetType().Name})");
                    continue;
                }

                playerEquipment.Equip(gear);
            }
        }

        
        if (!string.IsNullOrWhiteSpace(data.heldSlot1ItemId))
        {
            var item = ItemResolver.Resolve(data.heldSlot1ItemId);
            if (item is HoldableItemData holdable1)
                playerHeldEquipment.Equip(holdable1, HeldSlot.Slot1);
        }

        if (!string.IsNullOrWhiteSpace(data.heldSlot2ItemId))
        {
            var item = ItemResolver.Resolve(data.heldSlot2ItemId);
            if (item is HoldableItemData holdable2)
                playerHeldEquipment.Equip(holdable2, HeldSlot.Slot2);
        }

        if (data.activeHeldSlot >= 0)
        {
            SetActiveHeldSlot((HeldSlot)data.activeHeldSlot);
        }

        OnPlayerInventoryChanged?.Invoke();
        ClearSelection();
    }
}

// Saving player inventory and gear
[Serializable]
public class SaveInventoryData
{
    public List<InventoryItemSave> inventoryItems = new();
    public List<GearPairSave> gearSlots = new();

    public string heldSlot1ItemId;
    public string heldSlot2ItemId;
    public int activeHeldSlot = -1;
}

[Serializable]
public struct InventoryItemSave
{
    public string itemId;
    public int amount;
    public float durability;
}

// --- this could be inherited from InventoryItemSave
[Serializable]
public struct GearPairSave
{
    public GearData.GearSlot slot;
    public string gearId;
    public float durability;
}