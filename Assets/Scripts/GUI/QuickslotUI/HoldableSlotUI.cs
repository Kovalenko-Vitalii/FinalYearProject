using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HoldableSlotUI : MonoBehaviour
{
    [SerializeField] private Sprite defaultIcon;

    [SerializeField] private Image slotImage1;
    [SerializeField] private Image slotImage2;

    [SerializeField] private GameObject slotHighlight1;
    [SerializeField] private GameObject slotHighlight2;

    [SerializeField] private TextMeshProUGUI slotName1;
    [SerializeField] private TextMeshProUGUI slotName2;

    [SerializeField] private TextMeshProUGUI slotDetail1;
    [SerializeField] private TextMeshProUGUI slotDetail2;

    private void OnEnable()
    {
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.OnHeldEquipmentChanged += Refresh;
            InventoryManager.Instance.OnActiveHeldSlotChanged += HandleActiveSlotChanged;
            InventoryManager.Instance.OnPlayerInventoryChanged += Refresh;
        }

        Refresh();
    }

    private void OnDisable()
    {
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.OnHeldEquipmentChanged -= Refresh;
            InventoryManager.Instance.OnActiveHeldSlotChanged -= HandleActiveSlotChanged;
            InventoryManager.Instance.OnPlayerInventoryChanged += Refresh;
        }
    }

    private void HandleActiveSlotChanged(HeldSlot? _)
    {
        Refresh();
    }

    private void Refresh()
    {
        var inventory = InventoryManager.Instance;
        if (inventory == null)
            return;

        var slots = inventory.playerHeldEquipment.Slots;
        var activeSlot = inventory.ActiveHeldSlot;

        RefreshSlot(
            slots[HeldSlot.Slot1],
            HeldSlot.Slot1,
            activeSlot,
            slotImage1,
            slotHighlight1,
            slotName1,
            slotDetail1
        );

        RefreshSlot(
            slots[HeldSlot.Slot2],
            HeldSlot.Slot2,
            activeSlot,
            slotImage2,
            slotHighlight2,
            slotName2,
            slotDetail2
        );
    }

    private void RefreshSlot(
        InventoryItem item,
        HeldSlot slot,
        HeldSlot? activeSlot,
        Image slotImage,
        GameObject slotHighlight,
        TextMeshProUGUI slotName,
        TextMeshProUGUI slotDetail)
    {
        if (item == null || item.data == null)
        {
            slotImage.sprite = defaultIcon;
            slotName.text = "";
            slotDetail.text = "";
            slotHighlight.SetActive(false);
            return;
        }

        slotImage.sprite = item.data.icon != null ? item.data.icon : defaultIcon;
        slotName.text = item.data.itemName;
        slotDetail.text = BuildDetailText(item);
        slotHighlight.SetActive(activeSlot == slot);
    }

    private string BuildDetailText(InventoryItem item)
    {
        if (item == null || item.data == null)
            return "";

        if (item.data is HoldableFirearmData firearmData)
        {
            item.EnsureRuntimeState();

            int ammoInMag = item.firearmState != null ? item.firearmState.currentAmmoInMag : 0;
            int magCapacity = firearmData.magCapacity;

            int reserveAmmo = 0;
            var im = InventoryManager.Instance;
            if (im != null && firearmData.ammoItem != null)
                reserveAmmo = im.GetPlayerItemCount(firearmData.ammoItem);

            return $"{ammoInMag}/{magCapacity} ({reserveAmmo})";
        }

        if (item.HasDurability)
        {
            return $"{Mathf.CeilToInt(item.currentDurability)}";
        }

        return "";
    }
}
