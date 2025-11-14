using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GearSelectionUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Image centerIcon;
    [SerializeField] private Image leftPreviewIcon;
    [SerializeField] private Image rightPreviewIcon;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI descText;
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
    [SerializeField, TextArea] private string defaultDescription = "You have no gear for this slot.";


    private System.Collections.Generic.List<GearData> options = new();
    private int index = -1;

    private GearData.GearSlot activeSlot;
    private GearData equipped;
    private Inventory playerInv;
    private Equipment playerEq;

    private bool subscribed;

    private void Awake()
    {
        if (leftBtn) leftBtn.onClick.AddListener(OnLeft);
        if (rightBtn) rightBtn.onClick.AddListener(OnRight);
    }

    public void Open(GearData.GearSlot slot)
    {
        var im = InventoryManager.Instance;
        playerInv = im.playerInventory;
        playerEq = im.playerEquipment;

        activeSlot = slot;
        equipped = playerEq.GetEquipped(slot);

        BuildOptions();
        UpdateUI();

        Subscribe();
        RebuildAndRefresh();
    }

    private void OnLeft()
    {
        if (options.Count == 0) return;
        if (index > 0) index--;
        UpdateUI();
    }

    private void OnRight()
    {
        if (options.Count == 0) return;
        if (index < options.Count - 1) index++;
        UpdateUI();
    }

    public void UpdateUI()
    {
        bool hasList = options.Count > 0 && index >= 0;

        if (leftBtn) leftBtn.interactable = hasList && index > 0;
        if (rightBtn) rightBtn.interactable = hasList && index < options.Count - 1;

        if (!hasList && equipped == null)
        {
            ShowDefault();
            return;
        }

        if (!hasList)
        {
            SetImage(centerIcon, equipped ? equipped.icon : defaultIcon, Color.white);
            if (nameText) nameText.text = equipped != null ? equipped.itemName : defaultName;
            if (descText) descText.text = equipped != null ? equipped.description : defaultDescription;

            SetImage(leftPreviewIcon, null, previewHiddenColor);
            SetImage(rightPreviewIcon, null, previewHiddenColor);

            if (centerHighlight) centerHighlight.SetActive(equipped != null);
            if (statPanel) statPanel.Render(equipped);

            BindEquipForSelected(null);
            return;
        }


        var sel = options[index];

        SetImage(centerIcon, sel.icon, Color.white);
        if (nameText) nameText.text = sel.itemName;
        if (descText) descText.text = sel.description;

        if (index > 0)
            SetImage(leftPreviewIcon, options[index - 1].icon, previewDimColor);
        else
            SetImage(leftPreviewIcon, null, previewHiddenColor);

        if (index < options.Count - 1)
            SetImage(rightPreviewIcon, options[index + 1].icon, previewDimColor);
        else
            SetImage(rightPreviewIcon, null, previewHiddenColor);

        if (centerHighlight) centerHighlight.SetActive(equipped != null && ReferenceEquals(sel, equipped));
        if (statPanel) statPanel.Render(sel);

        BindEquipForSelected(sel);
    }


    private void SetImage(Image img, Sprite sprite, Color tint)
    {
        if (!img) return;
        img.sprite = sprite;
        img.color = tint;
        img.enabled = sprite != null || tint.a > 0f;
    }

    private void BindEquipForSelected(GearData selected)
    {
        if (!buttonEquip)
            return;

        buttonEquip.onClick.RemoveAllListeners();

        if (selected == null)
        {
            if (buttonEquip) buttonEquip.gameObject.SetActive(false);
            if (buttonDrop) buttonDrop.gameObject.SetActive(false);
            if (buttonActions) buttonActions.gameObject.SetActive(false);
            return;
        }

        var invItem = InventoryUtil.MakeItem(playerInv, selected);

        ActionBinder.BindFixedButtons(
            dropButton: buttonDrop,
            primaryButton: buttonEquip,
            actionsButton: buttonActions,
            invItem: invItem,
            source: playerInv,
            afterActionRefresh: () =>
            {
                equipped = playerEq.GetEquipped(activeSlot);
                RebuildAndRefresh();
            },
            primaryFallbackLabel: "Equip"
        );
    }


    private void BuildOptions()
    {
        options = new System.Collections.Generic.List<GearData>();

        if (equipped != null && equipped.slot == activeSlot)
            options.Add(equipped);

        var fromInv = playerInv.items
            .Where(i => i.data is GearData g && g.slot == activeSlot)
            .Select(i => (GearData)i.data)
            .Where(g => g != equipped);

        options.AddRange(fromInv.Distinct());
        index = options.Count > 0 ? 0 : -1;
    }

    private void Subscribe()
    {
        if (subscribed) return;
        if (playerInv != null) InventoryManager.Instance.OnPlayerInventoryChanged += OnInventoryChanged;
        if (playerEq != null) playerEq.OnChanged += OnEquipmentChanged;
        subscribed = true;
    }

    private void Unsubscribe()
    {
        if (!subscribed) return;
        if (InventoryManager.Instance != null)
            InventoryManager.Instance.OnPlayerInventoryChanged -= OnInventoryChanged;
        if (playerEq != null)
            playerEq.OnChanged -= OnEquipmentChanged;
        subscribed = false;
    }

    private void OnDestroy() => Unsubscribe();

    private void OnInventoryChanged() => RebuildAndRefresh();

    private void OnEquipmentChanged(GearData.GearSlot slot, GearData oldGear, GearData newGear)
    {
        if (slot != activeSlot) return;
        equipped = newGear;
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

        SetImage(leftPreviewIcon, null, previewHiddenColor);
        SetImage(rightPreviewIcon, null, previewHiddenColor);

        if (centerHighlight) centerHighlight.SetActive(false);

        if (statPanel) statPanel.Clear();

        if (buttonEquip) buttonEquip.gameObject.SetActive(false);
        if (buttonDrop) buttonDrop.gameObject.SetActive(false);
        if (buttonActions) buttonActions.gameObject.SetActive(false);
    }

}
