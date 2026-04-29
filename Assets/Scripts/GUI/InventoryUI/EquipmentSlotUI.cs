using UnityEngine;
using UnityEngine.UI;

public class EquipmentSlotUI : MonoBehaviour
{
    [SerializeField] private EquipmentSlotId slotId;
    [SerializeField] private Image icon;
    [SerializeField] private Sprite defaultIcon;
    [SerializeField] private Button button;
    [SerializeField] private GameObject activeHeldHighlight;

    private InventoryItem currentItem;

    public EquipmentSlotId SlotId => slotId;

    public void SetItem(InventoryItem item, bool isActiveHeld = false)
    {
        currentItem = item;

        if (icon != null)
            icon.sprite = item?.data?.icon != null ? item.data.icon : defaultIcon;

        if (activeHeldHighlight != null)
            activeHeldHighlight.SetActive(isActiveHeld);

        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(OnClicked);
        }
    }

    private void OnClicked()
    {
        var selection = Object.FindAnyObjectByType<EquipmentSelectionUI>();
        if (selection != null)
            selection.OpenSlot(slotId);

        SoundManager.Instance.PlayUI(
            UISoundId.ItemClick,
            currentItem?.data != null ? currentItem.data.onClickSound : null
        );
    }
}