using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class InventoryManager : MonoBehaviour, ISaveable
{
    public static InventoryManager Instance { get; private set; }
    public string SaveId => "PLAYER_INVENTORY";
    string TAG => SaveId;

    [Header("Settings")]
    [SerializeField] private int playerSlotLimit = 10;

    [Header("Test items")]
    [SerializeField] private ItemData[] initialItems;
    [SerializeField] private int[] initialAmounts;

    public Inventory playerInventory { get; private set; }
    public Equipment playerEquipment { get; private set; }
    public HeldEquipment playerHeldEquipment { get; private set; }

    public InventoryItem SelectedItem { get; private set; }
    public Inventory SourceInventory { get; private set; }
    public HeldSlot? ActiveHeldSlot { get; private set; }

    public event Action OnPlayerInventoryChanged;
    public event Action OnHeldEquipmentChanged;
    public event Action<HeldSlot?> OnActiveHeldSlotChanged;
    public event Action<InventoryItem, Inventory> OnSelectionChanged;

    private void Awake()
    {
        if (!TryInitializeSingleton())
            return;

        InitializeInventories();
        SubscribeToEvents();
    }

    private void Start()
    {
        AddInitialItems();
    }

    private bool TryInitializeSingleton()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return false;
        }

        Instance = this;
        return true;
    }

    private void InitializeInventories()
    {
        playerInventory = new Inventory(new PlayerInventoryPolicy(playerSlotLimit));
        playerEquipment = new Equipment();
        playerHeldEquipment = new HeldEquipment();
    }

    private void SubscribeToEvents()
    {
        playerInventory.OnChanged += () => OnPlayerInventoryChanged?.Invoke();
        playerHeldEquipment.OnChanged += () => OnHeldEquipmentChanged?.Invoke();
    }

    private void AddInitialItems()
    {
        if (initialItems == null)
            return;

        for (int i = 0; i < initialItems.Length; i++)
        {
            var itemData = initialItems[i];
            int amount = (initialAmounts != null && i < initialAmounts.Length) ? initialAmounts[i] : 1;

            if (itemData is GearData gearData)
            {
                playerEquipment.Equip(gearData);
                continue;
            }

            playerInventory.AddItem(itemData, amount, itemData.maxDurability);
        }
    }

    public void MoveItem(Inventory from, Inventory to, ItemData data, int amount, float currentDurability)
    {
        if (from == null || to == null || data == null || amount <= 0)
            return;

        var src = from.items.Find(i => i.data == data);
        if (src == null || src.amount <= 0)
            return;

        int canMove = Mathf.Min(src.amount, amount);

        int beforeToCount = GetItemCount(to, data);
        to.AddItem(data, canMove, currentDurability);
        int afterToCount = GetItemCount(to, data);

        int accepted = Mathf.Clamp(afterToCount - beforeToCount, 0, canMove);
        if (accepted <= 0)
            return;

        from.RemoveItem(data, accepted);
    }

    private int GetItemCount(Inventory inventory, ItemData data)
    {
        return inventory.items
            .Where(i => i.data == data)
            .Sum(i => i.amount);
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
            UnequipHeldItem(existingSlot);
            return true;
        }

        EquipHeldItem(holdable);
        return true;
    }

    private void UnequipHeldItem(HeldSlot existingSlot)
    {
        playerHeldEquipment.Unequip(existingSlot);

        if (ActiveHeldSlot == existingSlot)
            SetActiveHeldSlot(null);

        OnPlayerInventoryChanged?.Invoke();
    }

    private void EquipHeldItem(HoldableItemData holdable)
    {
        HeldSlot targetSlot = GetTargetHeldSlot();
        playerHeldEquipment.Equip(holdable, targetSlot);
        OnPlayerInventoryChanged?.Invoke();
    }

    private HeldSlot GetTargetHeldSlot()
    {
        var empty = playerHeldEquipment.GetFirstEmptySlot();
        if (empty.HasValue)
            return empty.Value;

        return ActiveHeldSlot ?? HeldSlot.Slot1;
    }

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

    public object CaptureState()
    {
        var data = new SaveInventoryData();

        CaptureInventoryState(data);
        CaptureGearState(data);
        CaptureHeldState(data);

        return data;
    }

    private void CaptureInventoryState(SaveInventoryData data)
    {
        foreach (var it in playerInventory.items)
        {
            if (it == null || it.data == null || it.amount <= 0)
                continue;

            data.inventoryItems.Add(new InventoryItemSave
            {
                itemId = it.data.id,
                amount = it.amount,
                durability = it.currentDurability
            });
        }
    }

    private void CaptureGearState(SaveInventoryData data)
    {
        foreach (var kv in playerEquipment.slots)
        {
            var gear = kv.Value;
            if (gear == null)
                continue;

            data.gearSlots.Add(new GearPairSave
            {
                slot = kv.Key,
                gearId = gear.id,
                durability = 0f
            });
        }
    }

    private void CaptureHeldState(SaveInventoryData data)
    {
        var held1 = playerHeldEquipment.GetEquipped(HeldSlot.Slot1);
        var held2 = playerHeldEquipment.GetEquipped(HeldSlot.Slot2);

        data.heldSlot1ItemId = held1 != null ? held1.id : null;
        data.heldSlot2ItemId = held2 != null ? held2.id : null;
        data.activeHeldSlot = ActiveHeldSlot.HasValue ? (int)ActiveHeldSlot.Value : -1;
    }

    public void RestoreState(object state)
    {
        var data = state as SaveInventoryData;

        ClearAllState();

        if (data == null)
        {
            FinishRestoreState();
            return;
        }

        RestoreInventoryState(data);
        RestoreGearState(data);
        RestoreHeldState(data);
        RestoreActiveHeldSlot(data);

        FinishRestoreState();
    }

    private void ClearAllState()
    {
        playerInventory.items.Clear();
        playerHeldEquipment.Clear();
        SetActiveHeldSlot(null);

        playerEquipment.Unequip(GearData.GearSlot.Head);
        playerEquipment.Unequip(GearData.GearSlot.Chest);
        playerEquipment.Unequip(GearData.GearSlot.Legs);
        playerEquipment.Unequip(GearData.GearSlot.Boots);
    }

    private void RestoreInventoryState(SaveInventoryData data)
    {
        if (data.inventoryItems == null)
            return;

        foreach (var s in data.inventoryItems)
        {
            var itemData = ItemResolver.Resolve(s.itemId);
            if (itemData == null)
            {
                GameLog.Warning(TAG, $"Unknown itemId '{s.itemId}'");
                continue;
            }

            playerInventory.AddItem(itemData, s.amount, s.durability);
        }
    }

    private void RestoreGearState(SaveInventoryData data)
    {
        if (data.gearSlots == null)
            return;

        foreach (var p in data.gearSlots)
        {
            if (string.IsNullOrWhiteSpace(p.gearId))
                continue;

            var item = ItemResolver.Resolve(p.gearId);
            if (item == null)
            {
                GameLog.Warning(TAG, $"Unknown gearId '{p.gearId}'");
                continue;
            }

            if (item is not GearData gear)
            {
                GameLog.Warning(TAG, $"itemId '{p.gearId}' is not GearData (got {item.GetType().Name})");
                continue;
            }

            playerEquipment.Equip(gear);
        }
    }

    private void RestoreHeldState(SaveInventoryData data)
    {
        RestoreHeldSlot(data.heldSlot1ItemId, HeldSlot.Slot1);
        RestoreHeldSlot(data.heldSlot2ItemId, HeldSlot.Slot2);
    }

    private void RestoreHeldSlot(string itemId, HeldSlot slot)
    {
        if (string.IsNullOrWhiteSpace(itemId))
            return;

        var item = ItemResolver.Resolve(itemId);
        if (item is HoldableItemData holdable)
            playerHeldEquipment.Equip(holdable, slot);
    }

    private void RestoreActiveHeldSlot(SaveInventoryData data)
    {
        if (data.activeHeldSlot >= 0)
            SetActiveHeldSlot((HeldSlot)data.activeHeldSlot);
    }

    private void FinishRestoreState()
    {
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