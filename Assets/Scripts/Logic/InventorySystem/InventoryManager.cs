using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static ItemData;

// This class is responsible for managing inventories, it stores player inventory, equipment and allows to interact with storage
public class InventoryManager : MonoBehaviour, ISaveable
{
    public static InventoryManager Instance { get; private set; }

    public string SaveId => "PLAYER_INVENTORY_V2";
    private string TAG => SaveId;

    [Header("Settings")]
    [SerializeField] private int playerSlotLimit = 10;

    public int maxSlots => playerInventory?.maxSlots ?? -1;
    public int currentSlots => playerInventory?.currentSlots ?? 0;

    [field: SerializeField] public Inventory playerInventory { get; private set; }
    public EquippedItems playerEquippedItems { get; private set; }

    public InventoryItem SelectedItem { get; private set; }
    public Inventory SourceInventory { get; private set; }
    public EquipmentSlotId? ActiveHeldSlot { get; private set; }

    public event Action OnPlayerInventoryChanged;
    public event Action<InventoryItem, Inventory> OnSelectionChanged;

    public event Action OnEquipmentChanged;
    public event Action<EquipmentSlotId, InventoryItem, InventoryItem> OnEquipmentSlotChanged;
    public event Action<EquipmentSlotId?> OnActiveHeldSlotChanged;

    public event Action OwnedItemsChanged;



    private void Awake()
    {
        if (!TryInitializeSingleton())
            return;

        InitializeInventories();
        SubscribeToEvents();
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
        playerEquippedItems = new EquippedItems(EquipmentSlots.All);
    }

    private void SubscribeToEvents()
    {
        playerInventory.OnChanged += () =>
        {
            OnPlayerInventoryChanged?.Invoke();
            OwnedItemsChanged?.Invoke();
        };

        playerEquippedItems.OnSlotChanged += (slot, oldItem, newItem) =>
        {
            OnEquipmentChanged?.Invoke();
            OnEquipmentSlotChanged?.Invoke(slot, oldItem, newItem);
            OwnedItemsChanged?.Invoke();
        };
    }

    // =========================
    // Selection
    // =========================
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

    // =========================
    // Queries
    // =========================
    public int GetPlayerItemCount(ItemData data)
    {
        if (playerInventory == null || data == null)
            return 0;

        return playerInventory.items
            .Where(i => i.data == data)
            .Sum(i => i.amount);
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

    public InventoryItem GetEquippedItem(EquipmentSlotId slot)
    {
        return playerEquippedItems.Get(slot);
    }

    public InventoryItem GetActiveHeldItem()
    {
        if (!ActiveHeldSlot.HasValue)
            return null;

        return playerEquippedItems.Get(ActiveHeldSlot.Value);
    }

    public void NotifyRuntimeItemStateChanged()
    {
        OnPlayerInventoryChanged?.Invoke();
        OnEquipmentChanged?.Invoke();
    }

    // =========================
    // Inventory transfer
    // =========================
    public void MoveItem(Inventory from, Inventory to, ItemData data, int amount, float currentDurability)
    {
        if (from == null || to == null || data == null || amount <= 0)
            return;

        var src = from.items.Find(i => i.data == data);
        if (src == null || src.amount <= 0)
            return;

        int canMove = Mathf.Min(src.amount, amount);
        int accepted = to.AddItemAndGetAccepted(data, canMove, currentDurability);

        if (accepted > 0)
            from.RemoveItem(data, accepted);
    }
    public bool TryMoveItemInstance(Inventory from, Inventory to, InventoryItem item)
    {
        if (from == null || to == null || item == null)
            return false;

        if (!from.RemoveItemInstance(item))
            return false;

        if (!to.TryAddItemInstance(item))
        {
            from.TryAddItemInstance(item);
            return false;
        }

        return true;
    }

    // =========================
    // Equip / Unequip
    // =========================
    public bool TryToggleEquipItem(Inventory source, InventoryItem item)
    {
        if (item == null || item.data is not IEquippableItemData)
            return false;

        if (playerEquippedItems.TryFindSlot(item, out var currentSlot))
            return TryUnequipItem(currentSlot, source ?? playerInventory);

        return TryEquipItem(source, item);
    }

    public bool TryEquipItem(Inventory source, InventoryItem item)
    {
        if (source == null || item == null || item.data is not IEquippableItemData equippable)
            return false;

        var preferredSlot = GetPreferredSlot(equippable);
        if (!preferredSlot.HasValue)
            return false;

        return TryEquipItem(source, item, preferredSlot.Value);
    }

    public bool TryEquipItem(Inventory source, InventoryItem item, EquipmentSlotId slot)
    {
        if (source == null || item == null || item.data is not IEquippableItemData equippable)
            return false;

        if (!playerEquippedItems.HasSlot(slot))
            return false;

        if (!equippable.AllowedSlots.Contains(slot))
            return false;

        if (!source.RemoveItemInstance(item))
            return false;

        InventoryItem displaced = playerEquippedItems.Equip(slot, item);

        if (displaced != null)
        {
            if (!source.TryAddItemInstance(displaced))
            {
                playerEquippedItems.Equip(slot, displaced);
                source.TryAddItemInstance(item);
                return false;
            }
        }

        if (EquipmentSlots.IsHeld(slot))
            SetActiveHeldSlot(slot);

        return true;
    }

    public bool TryUnequipItem(EquipmentSlotId slot, Inventory target)
    {
        if (target == null || !playerEquippedItems.HasSlot(slot))
            return false;

        InventoryItem item = playerEquippedItems.Unequip(slot);
        if (item == null)
            return false;

        if (!target.TryAddItemInstance(item))
        {
            playerEquippedItems.Equip(slot, item);
            return false;
        }

        if (ActiveHeldSlot == slot)
            SetActiveHeldSlot(null);

        return true;
    }

    // =========================
    // Held logic
    // =========================
    public void ToggleActiveHeldSlot(EquipmentSlotId slot)
    {
        if (!EquipmentSlots.IsHeld(slot))
            return;

        if (playerEquippedItems.Get(slot) == null)
            return;

        if (ActiveHeldSlot == slot)
            SetActiveHeldSlot(null);
        else
            SetActiveHeldSlot(slot);
    }

    public void SetActiveHeldSlot(EquipmentSlotId? slot)
    {
        if (slot.HasValue)
        {
            if (!EquipmentSlots.IsHeld(slot.Value))
                return;

            if (playerEquippedItems.Get(slot.Value) == null)
                slot = null;
        }

        if (ActiveHeldSlot == slot)
            return;

        ActiveHeldSlot = slot;
        OnActiveHeldSlotChanged?.Invoke(slot);
    }

    private EquipmentSlotId? GetPreferredSlot(IEquippableItemData equippable)
    {
        if (equippable == null || equippable.AllowedSlots == null || equippable.AllowedSlots.Count == 0)
            return null;

        foreach (var slot in equippable.AllowedSlots)
        {
            if (!EquipmentSlots.IsHeld(slot) && playerEquippedItems.Get(slot) == null)
                return slot;
        }

        var emptyHeld = playerEquippedItems.GetFirstEmpty(EquipmentSlots.Held);
        if (emptyHeld.HasValue && equippable.AllowedSlots.Contains(emptyHeld.Value))
            return emptyHeld.Value;

        if (ActiveHeldSlot.HasValue && equippable.AllowedSlots.Contains(ActiveHeldSlot.Value))
            return ActiveHeldSlot.Value;

        return equippable.AllowedSlots[0];
    }

    // =========================
    // For Quests
    // =========================

    public int GetOwnedAmountByItemId(string itemId)
    {
        if (string.IsNullOrWhiteSpace(itemId))
            return 0;

        int total = 0;

        foreach (var item in EnumerateOwnedItems())
        {
            if (item.data.id == itemId)
                total += item.amount;
        }

        return total;
    }

    public int GetOwnedAmountByTag(ItemTag tag)
    {
        if (tag == ItemTag.None)
            return 0;

        int total = 0;

        foreach (var item in EnumerateOwnedItems())
        {
            if ((item.data.Tags & tag) != 0)
                total += item.amount;
        }

        return total;
    }

    private IEnumerable<InventoryItem> EnumerateOwnedItems()
    {
        if (playerInventory != null)
        {
            foreach (var item in playerInventory.items)
            {
                if (item?.data != null && item.amount > 0)
                    yield return item;
            }
        }

        if (playerEquippedItems != null)
        {
            foreach (var item in playerEquippedItems.Slots.Values)
            {
                if (item?.data != null && item.amount > 0)
                    yield return item;
            }
        }
    }

    // =========================
    // Save / Load
    // =========================
    public object CaptureState()
    {
        var data = new SaveInventoryData();

        CaptureInventoryState(data);
        CaptureEquippedState(data);
        CaptureActiveHeldState(data);

        return data;
    }

    public void RestoreState(object state)
    {
        var data = state as SaveInventoryData;

        ResetToDefaultState();

        if (data == null)
        {
            FinishRestoreState();
            return;
        }

        RestoreInventoryState(data);
        RestoreEquippedState(data);
        RestoreActiveHeldState(data);

        FinishRestoreState();
    }

    private void CaptureInventoryState(SaveInventoryData data)
    {
        foreach (var item in playerInventory.items)
        {
            if (item == null || item.data == null || item.amount <= 0)
                continue;

            data.inventoryItems.Add(InventoryItemSave.FromRuntime(item));
        }
    }

    private void CaptureEquippedState(SaveInventoryData data)
    {
        foreach (var kv in playerEquippedItems.Slots)
        {
            if (kv.Value == null)
                continue;

            data.equippedSlots.Add(new EquippedSlotSave
            {
                slot = kv.Key,
                item = InventoryItemSave.FromRuntime(kv.Value)
            });
        }
    }

    private void CaptureActiveHeldState(SaveInventoryData data)
    {
        data.activeHeldSlot = ActiveHeldSlot.HasValue ? (int)ActiveHeldSlot.Value : -1;
    }

    public void ResetToDefaultState()
    {
        playerInventory.items.Clear();
        playerEquippedItems.Clear();
        SetActiveHeldSlot(null);
        ClearSelection();
    }

    private void RestoreInventoryState(SaveInventoryData data)
    {
        if (data.inventoryItems == null)
            return;

        foreach (var savedItem in data.inventoryItems)
        {
            var item = savedItem.ToRuntime();
            if (item == null)
            {
                GameLog.Warning(TAG, $"Unknown itemId '{savedItem.itemId}'");
                continue;
            }

            item.EnsureInstanceId();
            playerInventory.TryAddItemInstance(item);
        }
    }

    private void RestoreEquippedState(SaveInventoryData data)
    {
        if (data.equippedSlots == null)
            return;

        foreach (var savedSlot in data.equippedSlots)
        {
            if (!playerEquippedItems.HasSlot(savedSlot.slot))
                continue;

            if (string.IsNullOrWhiteSpace(savedSlot.item.itemId))
                continue;

            var item = savedSlot.item.ToRuntime();
            if (item == null)
                continue;

            item.EnsureInstanceId();
            playerEquippedItems.Equip(savedSlot.slot, item);
        }
    }

    private void RestoreActiveHeldState(SaveInventoryData data)
    {
        if (data.activeHeldSlot < 0)
            return;

        var slot = (EquipmentSlotId)data.activeHeldSlot;

        if (!EquipmentSlots.IsHeld(slot))
            return;

        if (playerEquippedItems.Get(slot) == null)
            return;

        SetActiveHeldSlot(slot);
    }

    private void FinishRestoreState()
    {
        OnPlayerInventoryChanged?.Invoke();
        OnEquipmentChanged?.Invoke();
        ClearSelection();
    }
}

// Saving player inventory and gear
[Serializable]
public class SaveInventoryData
{
    public List<InventoryItemSave> inventoryItems = new();
    public List<EquippedSlotSave> equippedSlots = new();
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

[Serializable]
public struct EquippedSlotSave
{
    public EquipmentSlotId slot;
    public InventoryItemSave item;
}