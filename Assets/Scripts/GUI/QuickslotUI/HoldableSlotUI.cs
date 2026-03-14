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
        }

        Refresh();
    }

    private void OnDisable()
    {
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.OnHeldEquipmentChanged -= Refresh;
            InventoryManager.Instance.OnActiveHeldSlotChanged -= HandleActiveSlotChanged;
        }
    }

    private void HandleActiveSlotChanged(HeldSlot? _)
    {
        Refresh();
    }

    private void Refresh()
    {
        var inventory = InventoryManager.Instance;
        if (inventory == null) return;

        var slots = inventory.playerHeldEquipment.Slots;
        var activeSlot = inventory.ActiveHeldSlot;

        ItemData item1 = slots[HeldSlot.Slot1];
        if (item1 != null)
        {
            slotImage1.sprite = item1.icon != null ? item1.icon : defaultIcon;
            slotName1.text = item1.itemName;
            slotDetail1.text = "";
            slotHighlight1.SetActive(activeSlot == HeldSlot.Slot1);
        }
        else
        {
            slotImage1.sprite = defaultIcon;
            slotName1.text = "";
            slotDetail1.text = "";
            slotHighlight1.SetActive(false);
        }

        ItemData item2 = slots[HeldSlot.Slot2];
        if (item2 != null)
        {
            slotImage2.sprite = item2.icon != null ? item2.icon : defaultIcon;
            slotName2.text = item2.itemName;
            slotDetail2.text = "";
            slotHighlight2.SetActive(activeSlot == HeldSlot.Slot2);
        }
        else
        {
            slotImage2.sprite = defaultIcon;
            slotName2.text = "";
            slotDetail2.text = "";
            slotHighlight2.SetActive(false);
        }
    }
}
