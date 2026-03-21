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

    // === Initializtion ===
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

    // === Initialization Helpers ===
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

    // === Selection ===
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

    // === Inventory Queries ===
    public int GetPlayerItemCount(ItemData data)
    {
        if (playerInventory == null || data == null)
            return 0;

        return GetItemCount(playerInventory, data);
    }
    public bool HasPlayerItems(ItemData data, int amount = 1)
    {
        if (data == null || amount <= 0)
            return false;

        return GetPlayerItemCount(data) >= amount;
    }
    public bool TryConsumePlayerItems(ItemData data, int amount = 1)
    {
        if (playerInventory == null || data == null || amount <= 0)
            return false;

        if (GetPlayerItemCount(data) < amount)
            return false;

        playerInventory.RemoveItem(data, amount);
        return true;
    }
    public void NotifyRuntimeItemStateChanged()
    {
        OnPlayerInventoryChanged?.Invoke();
    }


    // === Inventory Transfering ===
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

    // === Held item equip/active slot ===
    public bool TryToggleEquipHeldItem(InventoryItem inventoryItem)
    {
        if (inventoryItem == null || inventoryItem.data is not HoldableItemData)
            return false;

        if (playerHeldEquipment.TryFindSlot(inventoryItem, out HeldSlot existingSlot))
            return TryUnequipHeldFromSlot(existingSlot, playerInventory);

        HeldSlot targetSlot = GetTargetHeldSlot();
        return TryEquipHeldToSlot(playerInventory, inventoryItem, targetSlot);
    }
    private void UnequipHeldItem(HeldSlot existingSlot)
    {
        InventoryItem item = playerHeldEquipment.Unequip(existingSlot);

        if (item != null)
            playerInventory.TryAddItemInstance(item);

        if (ActiveHeldSlot == existingSlot)
            SetActiveHeldSlot(null);

        OnPlayerInventoryChanged?.Invoke();
    }
    private void EquipHeldItem(InventoryItem item)
    {
        HeldSlot targetSlot = GetTargetHeldSlot();

        if (playerInventory.RemoveItemInstance(item))
        {
            var old = playerHeldEquipment.Equip(item, targetSlot);
            if (old != null)
                playerInventory.TryAddItemInstance(old);
        }

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
        var item = playerHeldEquipment.GetEquippedItem(slot);
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
    public InventoryItem GetActiveHeldItem()
    {
        if (!ActiveHeldSlot.HasValue)
            return null;

        return playerHeldEquipment.GetEquippedItem(ActiveHeldSlot.Value);
    }
    public bool TryEquipHeldToSlot(Inventory source, InventoryItem item, HeldSlot slot)
    {
        if (source == null || item == null || item.data is not HoldableItemData)
            return false;

        if (!source.RemoveItemInstance(item))
            return false;

        InventoryItem old = playerHeldEquipment.Equip(item, slot);

        if (old != null)
            playerInventory.TryAddItemInstance(old);

        SetActiveHeldSlot(slot);
        OnPlayerInventoryChanged?.Invoke();
        return true;
    }
    public bool TryUnequipHeldFromSlot(HeldSlot slot, Inventory target)
    {
        if (target == null)
            return false;

        InventoryItem item = playerHeldEquipment.Unequip(slot);
        if (item == null)
            return false;

        bool added = target.TryAddItemInstance(item);
        if (!added)
        {
            playerHeldEquipment.Equip(item, slot);
            return false;
        }

        if (ActiveHeldSlot == slot)
            SetActiveHeldSlot(null);

        OnPlayerInventoryChanged?.Invoke();
        return true;
    }

    // === Save/Load ===
    public object CaptureState()
    {
        var data = new SaveInventoryData();

        CaptureInventoryState(data);
        CaptureGearState(data);
        CaptureHeldState(data);

        return data;
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

    // === Save Helpers ===
    private void CaptureInventoryState(SaveInventoryData data)
    {
        foreach (var it in playerInventory.items)
        {
            if (it == null || it.data == null || it.amount <= 0)
                continue;

            data.inventoryItems.Add(InventoryItemSave.FromRuntime(it));
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
        var held1 = playerHeldEquipment.GetEquippedItem(HeldSlot.Slot1);
        var held2 = playerHeldEquipment.GetEquippedItem(HeldSlot.Slot2);

        data.heldSlot1Item = held1 != null ? InventoryItemSave.FromRuntime(held1) : default;
        data.heldSlot2Item = held2 != null ? InventoryItemSave.FromRuntime(held2) : default;

        data.activeHeldSlot = ActiveHeldSlot.HasValue ? (int)ActiveHeldSlot.Value : -1;
    }

    // === Restore Helpers ===
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
            var item = s.ToRuntime();
            if (item == null)
            {
                GameLog.Warning(TAG, $"Unknown itemId '{s.itemId}'");
                continue;
            }

            item.EnsureInstanceId();
            playerInventory.TryAddItemInstance(item);
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
        RestoreHeldSlot(data.heldSlot1Item, HeldSlot.Slot1);
        RestoreHeldSlot(data.heldSlot2Item, HeldSlot.Slot2);
    }
    private void RestoreHeldSlot(InventoryItemSave savedItem, HeldSlot slot)
    {
        if (string.IsNullOrWhiteSpace(savedItem.itemId))
            return;

        var item = savedItem.ToRuntime();
        if (item == null)
            return;

        item.EnsureInstanceId();
        playerHeldEquipment.Equip(item, slot);
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

    public InventoryItemSave heldSlot1Item;
    public InventoryItemSave heldSlot2Item;

    public int activeHeldSlot = -1;
}

[Serializable]
public struct InventoryItemSave
{
    public string instanceId;
    public string itemId;
    public int amount;
    public float durability;

    public bool hasFirearmState;
    public int currentAmmoInMag;

    public static InventoryItemSave FromRuntime(InventoryItem item)
    {
        return new InventoryItemSave
        {
            instanceId = item != null ? item.instanceId : null,
            itemId = item?.data != null ? item.data.id : null,
            amount = item != null ? item.amount : 0,
            durability = item != null ? item.currentDurability : 0f,
            hasFirearmState = item?.firearmState != null,
            currentAmmoInMag = item?.firearmState != null ? item.firearmState.currentAmmoInMag : 0
        };
    }

    public InventoryItem ToRuntime()
    {
        var itemData = ItemResolver.Resolve(itemId);
        if (itemData == null)
            return null;

        var item = new InventoryItem(itemData, amount, durability);

        if (hasFirearmState)
        {
            item.firearmState = new FirearmRuntimeState
            {
                currentAmmoInMag = currentAmmoInMag
            };
        }

        item.EnsureRuntimeState();

        if (!string.IsNullOrWhiteSpace(instanceId))
            item.instanceId = instanceId;
        else
            item.EnsureInstanceId();

        return item;
    }
}

// --- this could be inherited from InventoryItemSave
[Serializable]
public struct GearPairSave
{
    public GearData.GearSlot slot;
    public string gearId;
    public float durability;
}