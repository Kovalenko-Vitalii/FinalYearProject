using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EquipmentSelectionUI : MonoBehaviour
{
    public enum Mode
    {
        Gear,
        Held
    }

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
    [SerializeField] private Color previewDimColor = new Color(1, 1, 1, 0.35f);
    [SerializeField] private Color previewHiddenColor = new Color(1, 1, 1, 0f);

    [Header("Defaults")]
    [SerializeField] private Sprite defaultIcon;
    [SerializeField] private string defaultName = "Nothing equipped";
    [SerializeField, TextArea] private string defaultDescription = "Nothing equipped in this slot.";

    private Mode mode;

    private GearData.GearSlot activeGearSlot;
    private HeldSlot activeHeldSlot;

    private Inventory playerInv;
    private Equipment playerEq;
    private HeldEquipment playerHeldEq;

    private GearData equippedGear;
    private InventoryItem equippedHeld;

    private System.Collections.Generic.List<GearData> gearOptions = new();
    private System.Collections.Generic.List<InventoryItem> heldOptions = new();

    private int index = -1;
    private bool subscribed;

    private void Awake()
    {
        if (leftBtn) leftBtn.onClick.AddListener(OnLeft);
        if (rightBtn) rightBtn.onClick.AddListener(OnRight);
    }

    public void OpenGear(GearData.GearSlot slot)
    {
        var im = InventoryManager.Instance;
        if (im == null)
            return;

        mode = Mode.Gear;
        playerInv = im.playerInventory;
        playerEq = im.playerEquipment;
        playerHeldEq = im.playerHeldEquipment;

        activeGearSlot = slot;
        equippedGear = playerEq.GetEquipped(slot);
        equippedHeld = null;

        BuildOptions();
        UpdateUI();

        Subscribe();
        RebuildAndRefresh();
    }

    public void OpenHeld(HeldSlot slot)
    {
        var im = InventoryManager.Instance;
        if (im == null)
            return;

        mode = Mode.Held;
        playerInv = im.playerInventory;
        playerEq = im.playerEquipment;
        playerHeldEq = im.playerHeldEquipment;

        activeHeldSlot = slot;
        equippedHeld = playerHeldEq.GetEquippedItem(slot);
        equippedGear = null;

        BuildOptions();
        UpdateUI();

        Subscribe();
        RebuildAndRefresh();
    }

    private void OnLeft()
    {
        if (GetOptionCount() == 0)
            return;

        if (index > 0)
            index--;

        UpdateUI();
    }

    private void OnRight()
    {
        if (GetOptionCount() == 0)
            return;

        if (index < GetOptionCount() - 1)
            index++;

        UpdateUI();
    }

    public void UpdateUI()
    {
        bool hasList = GetOptionCount() > 0 && index >= 0;

        if (leftBtn) leftBtn.interactable = hasList && index > 0;
        if (rightBtn) rightBtn.interactable = hasList && index < GetOptionCount() - 1;

        if (!hasList && !HasEquipped())
        {
            ShowDefault();
            return;
        }

        if (!hasList)
        {
            ShowEquippedOnly();
            return;
        }

        SetImage(centerIcon, GetSelectedIcon(), Color.white);

        if (nameText) nameText.text = GetSelectedName();
        if (descText) descText.text = GetSelectedDescription();
        if (detailText) detailText.text = GetSelectedDetail();

        SetImage(leftPreviewIcon, GetPreviewSprite(index - 1), index > 0 ? previewDimColor : previewHiddenColor);
        SetImage(rightPreviewIcon, GetPreviewSprite(index + 1), index < GetOptionCount() - 1 ? previewDimColor : previewHiddenColor);

        if (centerHighlight)
            centerHighlight.SetActive(IsSelectedEquipped());

        RenderStatsForCurrentSelection();
        BindButtonsForSelection();
    }

    private void ShowEquippedOnly()
    {
        SetImage(centerIcon, GetEquippedIcon(), Color.white);

        if (nameText) nameText.text = GetEquippedName();
        if (descText) descText.text = GetEquippedDescription();
        if (detailText) detailText.text = GetEquippedDetail();

        SetImage(leftPreviewIcon, null, previewHiddenColor);
        SetImage(rightPreviewIcon, null, previewHiddenColor);

        if (centerHighlight)
            centerHighlight.SetActive(HasEquipped());

        RenderStatsForEquippedOnly();
        BindButtonsForEquippedOnly();
    }

    private void BindButtonsForSelection()
    {
        ClearButtons();

        if (mode == Mode.Gear)
        {
            GearData selected = GetSelectedGear();
            if (selected == null)
                return;

            InventoryItem invItem = InventoryUtil.MakeItem(playerInv, selected);

            ActionBinder.BindFixedButtons(
                dropButton: buttonDrop,
                primaryButton: buttonEquip,
                actionsButton: buttonActions,
                invItem: invItem,
                source: playerInv,
                afterActionRefresh: () =>
                {
                    equippedGear = playerEq.GetEquipped(activeGearSlot);
                    RebuildAndRefresh();
                },
                primaryFallbackLabel: "Equip"
            );

            return;
        }

        if (mode == Mode.Held)
        {
            InventoryItem selected = GetSelectedHeld();
            if (selected == null)
                return;

            ActionBinder.BindFixedButtons(
                dropButton: buttonDrop,
                primaryButton: buttonEquip,
                actionsButton: buttonActions,
                invItem: selected,
                source: playerInv,
                afterActionRefresh: () =>
                {
                    equippedHeld = playerHeldEq.GetEquippedItem(activeHeldSlot);
                    RebuildAndRefresh();
                },
                primaryFallbackLabel: "Equip"
            );

            if (buttonEquip != null)
            {
                buttonEquip.onClick.RemoveAllListeners();
                buttonEquip.gameObject.SetActive(true);
                buttonEquip.interactable = true;

                var label = buttonEquip.GetComponentInChildren<TextMeshProUGUI>(true);
                if (label) label.text = "Equip";

                buttonEquip.onClick.AddListener(() =>
                {
                    InventoryManager.Instance.TryEquipHeldToSlot(playerInv, selected, activeHeldSlot);
                    equippedHeld = playerHeldEq.GetEquippedItem(activeHeldSlot);

                    var gearUI = Object.FindAnyObjectByType<GearUI>();
                    if (gearUI) gearUI.Refresh();

                    foreach (var invUI in Object.FindObjectsByType<InventoryUI>(FindObjectsSortMode.None))
                        invUI.Refresh();

                    RebuildAndRefresh();
                });
            }
        }
    }

    private void BindButtonsForEquippedOnly()
    {
        ClearButtons();

        if (mode == Mode.Gear)
        {
            if (equippedGear == null || buttonEquip == null)
                return;

            buttonEquip.gameObject.SetActive(true);
            buttonEquip.interactable = true;

            var label = buttonEquip.GetComponentInChildren<TextMeshProUGUI>(true);
            if (label) label.text = "Unequip";

            buttonEquip.onClick.AddListener(() =>
            {
                InventoryManager.Instance.playerEquipment.Unequip(activeGearSlot);
                playerInv.AddItem(equippedGear, 1, equippedGear.maxDurability);
                equippedGear = playerEq.GetEquipped(activeGearSlot);
                RebuildAndRefresh();
            });

            return;
        }

        if (mode == Mode.Held)
        {
            if (equippedHeld == null || buttonEquip == null)
                return;

            buttonEquip.gameObject.SetActive(true);
            buttonEquip.interactable = true;

            var label = buttonEquip.GetComponentInChildren<TextMeshProUGUI>(true);
            if (label) label.text = "Unequip";

            buttonEquip.onClick.AddListener(() =>
            {
                InventoryManager.Instance.TryUnequipHeldFromSlot(activeHeldSlot, playerInv);
                equippedHeld = playerHeldEq.GetEquippedItem(activeHeldSlot);
                RebuildAndRefresh();
            });

            return;
        }
    }

    private void BuildOptions()
    {
        gearOptions.Clear();
        heldOptions.Clear();

        if (mode == Mode.Gear)
        {
            if (equippedGear != null && equippedGear.slot == activeGearSlot)
                gearOptions.Add(equippedGear);

            var fromInv = playerInv.items
                .Where(i => i.data is GearData g && g.slot == activeGearSlot)
                .Select(i => (GearData)i.data)
                .Where(g => g != equippedGear);

            gearOptions.AddRange(fromInv.Distinct());
        }
        else
        {
            var fromInv = playerInv.items
                .Where(i => i != null && i.data is HoldableItemData);

            heldOptions.AddRange(fromInv);
        }

        index = GetOptionCount() > 0 ? 0 : -1;
    }

    private int GetOptionCount()
    {
        return mode == Mode.Gear ? gearOptions.Count : heldOptions.Count;
    }

    private bool HasEquipped()
    {
        return mode == Mode.Gear ? equippedGear != null : equippedHeld != null;
    }

    private GearData GetSelectedGear()
    {
        if (mode != Mode.Gear || index < 0 || index >= gearOptions.Count)
            return null;

        return gearOptions[index];
    }

    private InventoryItem GetSelectedHeld()
    {
        if (mode != Mode.Held || index < 0 || index >= heldOptions.Count)
            return null;

        return heldOptions[index];
    }

    private Sprite GetSelectedIcon()
    {
        if (mode == Mode.Gear)
            return GetSelectedGear()?.icon;

        return GetSelectedHeld()?.data?.icon;
    }

    private string GetSelectedName()
    {
        if (mode == Mode.Gear)
            return GetSelectedGear()?.itemName ?? defaultName;

        return GetSelectedHeld()?.data?.itemName ?? defaultName;
    }

    private string GetSelectedDescription()
    {
        if (mode == Mode.Gear)
            return GetSelectedGear()?.description ?? defaultDescription;

        return GetSelectedHeld()?.data?.description ?? defaultDescription;
    }

    private string GetSelectedDetail()
    {
        if (mode == Mode.Gear)
            return "";

        return BuildHeldDetailText(GetSelectedHeld());
    }

    private bool IsSelectedEquipped()
    {
        if (mode == Mode.Gear)
        {
            GearData selected = GetSelectedGear();
            return equippedGear != null && ReferenceEquals(selected, equippedGear);
        }

        InventoryItem selectedHeld = GetSelectedHeld();
        return equippedHeld != null && ReferenceEquals(selectedHeld, equippedHeld);
    }

    private Sprite GetPreviewSprite(int previewIndex)
    {
        if (previewIndex < 0 || previewIndex >= GetOptionCount())
            return null;

        if (mode == Mode.Gear)
            return gearOptions[previewIndex]?.icon;

        return heldOptions[previewIndex]?.data?.icon;
    }

    private Sprite GetEquippedIcon()
    {
        if (mode == Mode.Gear)
            return equippedGear != null ? equippedGear.icon : defaultIcon;

        return equippedHeld != null && equippedHeld.data != null ? equippedHeld.data.icon : defaultIcon;
    }

    private string GetEquippedName()
    {
        if (mode == Mode.Gear)
            return equippedGear != null ? equippedGear.itemName : defaultName;

        return equippedHeld != null && equippedHeld.data != null ? equippedHeld.data.itemName : defaultName;
    }

    private string GetEquippedDescription()
    {
        if (mode == Mode.Gear)
            return equippedGear != null ? equippedGear.description : defaultDescription;

        return equippedHeld != null && equippedHeld.data != null ? equippedHeld.data.description : defaultDescription;
    }

    private string GetEquippedDetail()
    {
        if (mode == Mode.Gear)
            return "";

        return BuildHeldDetailText(equippedHeld);
    }

    private string BuildHeldDetailText(InventoryItem item)
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

    private void RenderStatsForCurrentSelection()
    {
        if (statPanel == null)
            return;

        if (mode == Mode.Gear)
        {
            GearData selected = GetSelectedGear();
            if (selected == null)
            {
                statPanel.Clear();
                return;
            }

            var invItem = InventoryUtil.MakeItem(playerInv, selected);
            if (invItem != null)
                statPanel.Render(invItem);
            else
                statPanel.Render(selected);

            return;
        }

        InventoryItem heldSelected = GetSelectedHeld();
        if (heldSelected != null)
            statPanel.Render(heldSelected);
        else
            statPanel.Clear();
    }

    private void RenderStatsForEquippedOnly()
    {
        if (statPanel == null)
            return;

        if (mode == Mode.Gear)
        {
            if (equippedGear != null)
            {
                var invItem = InventoryUtil.MakeItem(playerInv, equippedGear);
                if (invItem != null)
                    statPanel.Render(invItem);
                else
                    statPanel.Render(equippedGear);
            }
            else
            {
                statPanel.Clear();
            }

            return;
        }

        if (equippedHeld != null)
            statPanel.Render(equippedHeld);
        else
            statPanel.Clear();
    }

    private void SetImage(Image img, Sprite sprite, Color tint)
    {
        if (!img) return;
        img.sprite = sprite;
        img.color = tint;
        img.enabled = sprite != null || tint.a > 0f;
    }

    private void ClearButtons()
    {
        if (buttonEquip != null)
        {
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

    private void Subscribe()
    {
        if (subscribed)
            return;

        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.OnPlayerInventoryChanged += OnInventoryChanged;
            InventoryManager.Instance.OnHeldEquipmentChanged += OnHeldChanged;
        }

        if (playerEq != null)
            playerEq.OnChanged += OnGearChanged;

        subscribed = true;
    }

    private void Unsubscribe()
    {
        if (!subscribed)
            return;

        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.OnPlayerInventoryChanged -= OnInventoryChanged;
            InventoryManager.Instance.OnHeldEquipmentChanged -= OnHeldChanged;
        }

        if (playerEq != null)
            playerEq.OnChanged -= OnGearChanged;

        subscribed = false;
    }

    private void OnDestroy()
    {
        Unsubscribe();
    }

    private void OnInventoryChanged()
    {
        RebuildAndRefresh();
    }

    private void OnHeldChanged()
    {
        if (mode != Mode.Held)
            return;

        equippedHeld = playerHeldEq != null ? playerHeldEq.GetEquippedItem(activeHeldSlot) : null;
        RebuildAndRefresh();
    }

    private void OnGearChanged(GearData.GearSlot slot, GearData oldGear, GearData newGear)
    {
        if (mode != Mode.Gear || slot != activeGearSlot)
            return;

        equippedGear = newGear;
        RebuildAndRefresh();
    }

    private void RebuildAndRefresh()
    {
        BuildOptions();
        UpdateUI();
    }

    private void ShowDefault()
    {
        SetImage(centerIcon, defaultIcon, Color.white);

        if (nameText) nameText.text = defaultName;
        if (descText) descText.text = defaultDescription;
        if (detailText) detailText.text = "";

        SetImage(leftPreviewIcon, null, previewHiddenColor);
        SetImage(rightPreviewIcon, null, previewHiddenColor);

        if (centerHighlight)
            centerHighlight.SetActive(false);

        if (statPanel)
            statPanel.Clear();

        ClearButtons();
    }
}