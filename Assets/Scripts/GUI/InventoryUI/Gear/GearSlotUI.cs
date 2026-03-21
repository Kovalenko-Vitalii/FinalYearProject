using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GearSlotUI : MonoBehaviour
{
    [SerializeField] private Image icon;

    [SerializeField] private GearData currentGearData;
    public Sprite defaultIcon;
    public GearSlot slot;

    [SerializeField] private Button button;


    public GearData.GearSlot ToGearSlot()
    {
        switch (slot)
        {
            case GearSlot.Head: return GearData.GearSlot.Head;
            case GearSlot.Chest: return GearData.GearSlot.Chest;
            case GearSlot.Legs: return GearData.GearSlot.Legs;
            case GearSlot.Shoes: return GearData.GearSlot.Boots;
            default: return GearData.GearSlot.Head;
        }
    }
    public void SetItem(GearData item)
    {
        currentGearData = item;
        if (item != null)
        {
            if (icon != null)
                icon.sprite = item.icon;
        }
        else
        {
            if (defaultIcon != null)
                icon.sprite = defaultIcon;
        }

        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => OnItemClicked());
        }
    }

    public enum GearSlot
    {
        Head,
        Chest,
        Legs,
        Shoes,
    }

    private void OnItemClicked()
    {
        var selection = Object.FindAnyObjectByType<EquipmentSelectionUI>();
        if (selection != null)
            selection.OpenGear(ToGearSlot());

        SoundManager.Instance.PlayUI(UISoundId.ItemClick, currentGearData != null ? currentGearData.onClickSound : null);
    }
}
