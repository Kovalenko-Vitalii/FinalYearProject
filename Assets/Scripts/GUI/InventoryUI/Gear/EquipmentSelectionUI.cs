using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EquipmentSelectionUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Image centerIcon;
    [SerializeField] private Image leftPreviewIcon;
    [SerializeField] private Image rightPreviewIcon;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI descText;
    [SerializeField] private TextMeshProUGUI detailText;
    [SerializeField] private Button leftBtn;
    [SerializeField] private Button rightBtn;
    [SerializeField] private Button buttonEquip;
    [SerializeField] private Button buttonActions;
    [SerializeField] private Button buttonDrop;
    [SerializeField] private GameObject centerHighlight;

    [Header("Stats UI")]
    [SerializeField] private StatPanelRenderer statPanel;

    [Header("Preview")]
    [SerializeField] private Color previewDimColor = new Color(1f, 1f, 1f, 0.35f);
    [SerializeField] private Color previewHiddenColor = new Color(1f, 1f, 1f, 0f);

    [Header("Defaults")]
    [SerializeField] private Sprite defaultIcon;
    [SerializeField] private string defaultName = "Nothing equipped";
    [SerializeField, TextArea] private string defaultDescription = "Nothing equipped in this slot.";

    private EquipmentSlotId activeSlot;
    private Inventory playerInv;
    private InventoryItem equippedItem;

    private readonly List<InventoryItem> options = new();

    private int index = -1;
    private bool subscribed;
    private bool hasOpenedSlot;

    private void Awake()
    {
        if (leftBtn != null)
            leftBtn.onClick.AddListener(OnLeft);

        if (rightBtn != null)
            rightBtn.onClick.AddListener(OnRight);
    }

    public void OpenSlot(EquipmentSlotId slot)
    {
        var im = InventoryManager.Instance;
        if (im == null)
            return;

        hasOpenedSlot = true;
        activeSlot = slot;
        playerInv = im.playerInventory;
        equippedItem = im.GetEquippedItem(slot);

        Subscribe();
        RebuildAndRefresh();
    }

    private void OnLeft()
    {
        if (options.Count == 0)
            return;

        if (index > 0)
            index--;

        UpdateUI();
    }

    private void OnRight()
    {
        if (options.Count == 0)
            return;

        if (index < options.Count - 1)
            index++;

        UpdateUI();
    }

    private void RebuildAndRefresh()
    {
        BuildOptions();
        UpdateUI();
    }

    private void BuildOptions()
    {
        string previouslySelectedInstanceId = GetSelectedItem()?.instanceId;

        options.Clear();

        if (!hasOpenedSlot || playerInv == null)
        {
            index = -1;
            return;
        }

        if (equippedItem != null)
            options.Add(equippedItem);

        var fromInventory = playerInv.items
            .Where(i => i != null)
            .Where(CanBeEquippedToActiveSlot)
            .Where(i => !ReferenceEquals(i, equippedItem));

        options.AddRange(fromInventory);

        if (options.Count == 0)
        {
            index = -1;
            return;
        }

        if (!string.IsNullOrWhiteSpace(previouslySelectedInstanceId))
        {
            int foundIndex = options.FindIndex(i => i != null && i.instanceId == previouslySelectedInstanceId);
            if (foundIndex >= 0)
            {
                index = foundIndex;
                return;
            }
        }

        if (equippedItem != null)
        {
            int equippedIndex = options.FindIndex(i => ReferenceEquals(i, equippedItem));
            if (equippedIndex >= 0)
            {
                index = equippedIndex;
                return;
            }
        }

        index = Mathf.Clamp(index, 0, options.Count - 1);
        if (index < 0)
            index = 0;
    }

    private bool CanBeEquippedToActiveSlot(InventoryItem item)
    {
        return item != null &&
               item.data is IEquippableItemData equippable &&
               equippable.AllowedSlots != null &&
               equippable.AllowedSlots.Contains(activeSlot);
    }

    public void UpdateUI()
    {
        bool hasSelection = options.Count > 0 && index >= 0 && index < options.Count;

        if (leftBtn != null)
            leftBtn.interactable = hasSelection && index > 0;

        if (rightBtn != null)
            rightBtn.interactable = hasSelection && index < options.Count - 1;

        if (!hasSelection && equippedItem == null)
        {
            ShowDefault();
            return;
        }

        if (!hasSelection)
        {
            ShowEquippedOnly();
            return;
        }

        InventoryItem selected = GetSelectedItem();

        SetImage(centerIcon, selected?.data?.icon ?? defaultIcon, Color.white);

        if (nameText != null)
            nameText.text = selected?.data?.itemName ?? defaultName;

        if (descText != null)
            descText.text = selected?.data?.description ?? defaultDescription;

        if (detailText != null)
            detailText.text = BuildDetailText(selected);

        SetImage(
            leftPreviewIcon,
            GetPreviewSprite(index - 1),
            index > 0 ? previewDimColor : previewHiddenColor
        );

        SetImage(
            rightPreviewIcon,
            GetPreviewSprite(index + 1),
            index < options.Count - 1 ? previewDimColor : previewHiddenColor
        );

        if (centerHighlight != null)
            centerHighlight.SetActive(IsSelectedEquipped());

        RenderStats(selected);
        BindButtons(selected);
    }

    private void ShowEquippedOnly()
    {
        SetImage(centerIcon, equippedItem?.data?.icon ?? defaultIcon, Color.white);

        if (nameText != null)
            nameText.text = equippedItem?.data?.itemName ?? defaultName;

        if (descText != null)
            descText.text = equippedItem?.data?.description ?? defaultDescription;

        if (detailText != null)
            detailText.text = BuildDetailText(equippedItem);

        SetImage(leftPreviewIcon, null, previewHiddenColor);
        SetImage(rightPreviewIcon, null, previewHiddenColor);

        if (centerHighlight != null)
            centerHighlight.SetActive(equippedItem != null);

        RenderStats(equippedItem);
        BindButtons(equippedItem);
    }

    private void BindButtons(InventoryItem selected)
    {
        ClearButtons();

        if (selected == null || playerInv == null)
            return;

        if (IsSelectedEquipped())
        {
            if (buttonEquip != null)
            {
                buttonEquip.gameObject.SetActive(true);
                buttonEquip.interactable = true;
                buttonEquip.onClick.RemoveAllListeners();

                var label = buttonEquip.GetComponentInChildren<TextMeshProUGUI>(true);
                if (label != null)
                    label.text = "Unequip";

                var hold = buttonEquip.GetComponent<HoldToUse>();
                hold?.ClearBinding();

                buttonEquip.onClick.AddListener(() =>
                {
                    var im = InventoryManager.Instance;
                    if (im == null)
                        return;

                    im.TryUnequipItem(activeSlot, playerInv);
                    equippedItem = im.GetEquippedItem(activeSlot);
                    RebuildAndRefresh();
                });
            }

            return;
        }

        ActionBinder.BindFixedButtons(
            dropButton: buttonDrop,
            primaryButton: buttonEquip,
            actionsButton: buttonActions,
            invItem: selected,
            source: playerInv,
            afterActionRefresh: () =>
            {
                var im = InventoryManager.Instance;
                if (im != null)
                    equippedItem = im.GetEquippedItem(activeSlot);

                RebuildAndRefresh();
            },
            primaryFallbackLabel: "Equip"
        );

        if (buttonEquip != null)
        {
            buttonEquip.gameObject.SetActive(true);
            buttonEquip.interactable = true;
            buttonEquip.onClick.RemoveAllListeners();

            var label = buttonEquip.GetComponentInChildren<TextMeshProUGUI>(true);
            if (label != null)
                label.text = "Equip";

            var hold = buttonEquip.GetComponent<HoldToUse>();
            hold?.ClearBinding();

            buttonEquip.onClick.AddListener(() =>
            {
                var im = InventoryManager.Instance;
                if (im == null)
                    return;

                im.TryEquipItem(playerInv, selected, activeSlot);
                equippedItem = im.GetEquippedItem(activeSlot);
                RebuildAndRefresh();
            });
        }
    }

    private InventoryItem GetSelectedItem()
    {
        if (index < 0 || index >= options.Count)
            return null;

        return options[index];
    }

    private bool IsSelectedEquipped()
    {
        InventoryItem selected = GetSelectedItem();
        return selected != null && equippedItem != null && ReferenceEquals(selected, equippedItem);
    }

    private Sprite GetPreviewSprite(int previewIndex)
    {
        if (previewIndex < 0 || previewIndex >= options.Count)
            return null;

        return options[previewIndex]?.data?.icon;
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

    private void RenderStats(InventoryItem item)
    {
        if (statPanel == null)
            return;

        if (item != null)
            statPanel.Render(item);
        else
            statPanel.Clear();
    }

    private void SetImage(Image img, Sprite sprite, Color tint)
    {
        if (img == null)
            return;

        img.sprite = sprite;
        img.color = tint;
        img.enabled = sprite != null || tint.a > 0f;
    }

    private void ClearButtons()
    {
        if (buttonEquip != null)
        {
            var hold = buttonEquip.GetComponent<HoldToUse>();
            hold?.ClearBinding();

            buttonEquip.onClick.RemoveAllListeners();
            buttonEquip.gameObject.SetActive(false);
        }

        if (buttonDrop != null)
        {
            buttonDrop.onClick.RemoveAllListeners();
            buttonDrop.gameObject.SetActive(false);
        }

        if (buttonActions != null)
        {
            buttonActions.onClick.RemoveAllListeners();
            buttonActions.gameObject.SetActive(false);
        }
    }

    private void ShowDefault()
    {
        SetImage(centerIcon, defaultIcon, Color.white);

        if (nameText != null)
            nameText.text = defaultName;

        if (descText != null)
            descText.text = defaultDescription;

        if (detailText != null)
            detailText.text = "";

        SetImage(leftPreviewIcon, null, previewHiddenColor);
        SetImage(rightPreviewIcon, null, previewHiddenColor);

        if (centerHighlight != null)
            centerHighlight.SetActive(false);

        if (statPanel != null)
            statPanel.Clear();

        ClearButtons();
    }

    private void Subscribe()
    {
        if (subscribed)
            return;

        var im = InventoryManager.Instance;
        if (im != null)
        {
            im.OnPlayerInventoryChanged += OnInventoryChanged;
            im.OnEquipmentSlotChanged += OnEquipmentSlotChanged;
        }

        subscribed = true;
    }

    private void Unsubscribe()
    {
        if (!subscribed)
            return;

        var im = InventoryManager.Instance;
        if (im != null)
        {
            im.OnPlayerInventoryChanged -= OnInventoryChanged;
            im.OnEquipmentSlotChanged -= OnEquipmentSlotChanged;
        }

        subscribed = false;
    }

    private void OnDestroy()
    {
        Unsubscribe();
    }

    private void OnInventoryChanged()
    {
        if (!hasOpenedSlot)
            return;

        var im = InventoryManager.Instance;
        if (im != null)
            equippedItem = im.GetEquippedItem(activeSlot);

        RebuildAndRefresh();
    }

    private void OnEquipmentSlotChanged(EquipmentSlotId slot, InventoryItem oldItem, InventoryItem newItem)
    {
        if (!hasOpenedSlot || slot != activeSlot)
            return;

        equippedItem = newItem;
        RebuildAndRefresh();
    }
}