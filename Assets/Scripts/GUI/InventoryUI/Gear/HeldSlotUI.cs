using UnityEngine;
using UnityEngine.UI;

public class HeldSlotUI : MonoBehaviour
{
    [SerializeField] private Image icon;
    [SerializeField] private InventoryItem currentHeldItem;
    [SerializeField] private Sprite defaultIcon;
    [SerializeField] private HeldSlot slot;
    [SerializeField] private Button button;

    public void SetItem(InventoryItem item)
    {
        currentHeldItem = item;

        if (icon != null)
        {
            icon.sprite = item != null && item.data != null
                ? item.data.icon
                : defaultIcon;
        }

        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(OnItemClicked);
        }
    }

    private void OnItemClicked()
    {
        var selection = Object.FindAnyObjectByType<EquipmentSelectionUI>();
        if (selection != null)
            selection.OpenHeld(slot);

        SoundManager.Instance.PlayUI(
            UISoundId.ItemClick,
            null
        );
    }
}