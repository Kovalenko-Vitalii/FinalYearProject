using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FPSHoldableSlotsUI : MonoBehaviour
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
        var im = InventoryManager.Instance;
        if (im != null)
        {
            im.OnEquipmentChanged += Refresh;
            im.OnActiveHeldSlotChanged += HandleActiveSlotChanged;
            im.OnPlayerInventoryChanged += Refresh;
        }

        Refresh();
    }

    private void OnDisable()
    {
        var im = InventoryManager.Instance;
        if (im != null)
        {
            im.OnEquipmentChanged -= Refresh;
            im.OnActiveHeldSlotChanged -= HandleActiveSlotChanged;
            im.OnPlayerInventoryChanged -= Refresh;
        }
    }

    private void HandleActiveSlotChanged(EquipmentSlotId? _)
    {
        Refresh();
    }

    private void Refresh()
    {
        var im = InventoryManager.Instance;
        if (im == null)
            return;

        RefreshSlot(
            im.GetEquippedItem(EquipmentSlotId.Held1),
            EquipmentSlotId.Held1,
            im.ActiveHeldSlot,
            slotImage1,
            slotHighlight1,
            slotName1,
            slotDetail1
        );

        RefreshSlot(
            im.GetEquippedItem(EquipmentSlotId.Held2),
            EquipmentSlotId.Held2,
            im.ActiveHeldSlot,
            slotImage2,
            slotHighlight2,
            slotName2,
            slotDetail2
        );
    }

    private void RefreshSlot(
        InventoryItem item,
        EquipmentSlotId slot,
        EquipmentSlotId? activeSlot,
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
            int reserveAmmo = 0;

            var im = InventoryManager.Instance;
            if (im != null && firearmData.ammoItem != null)
                reserveAmmo = im.GetPlayerItemCount(firearmData.ammoItem);

            return $"{ammoInMag}/{firearmData.magCapacity} ({reserveAmmo})";
        }

        if (item.HasDurability)
            return $"{Mathf.CeilToInt(item.currentDurability)}";

        return "";
    }
}