using System.Collections.Generic;
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
    [SerializeField] private Button equipBtn;
    [SerializeField] private GameObject centerHighlight;

    [Header("Preview")]
    [SerializeField] private Color previewDimColor = new Color(1, 1, 1, 0.35f);
    [SerializeField] private Color previewHiddenColor = new Color(1, 1, 1, 0f);

    [Header("Stats UI")]
    [SerializeField] private Transform statRoot;
    [SerializeField] private StatWidget statPrefab;
    [SerializeField] private StatLibrary statLibrary;
    [SerializeField] private Color positiveColor = Color.green;
    [SerializeField] private Color negativeColor = Color.red;

    private readonly List<StatWidget> statPool = new();

    private List<GearData> options = new();
    private int index = -1;

    private GearData.GearSlot activeSlot;
    private GearData equipped;
    private Inventory playerInv;
    private Equipment playerEq;

    private bool subscribed;

    private void Awake()
    {
        // Adding listeners
        if (leftBtn) leftBtn.onClick.AddListener(OnLeft);
        if (rightBtn) rightBtn.onClick.AddListener(OnRight);
        if (equipBtn) equipBtn.onClick.AddListener(OnEquip);
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

    private void OnEquip()
    {
        if (options.Count == 0 || index < 0) return;

        var selected = options[index];

        if (equipped == selected) return;

        playerInv.RemoveItem(selected, 1);

        var oldGear = playerEq.Equip(selected);
        if (oldGear != null)
            playerInv.AddItem(oldGear, 1);

        equipped = selected;

        BuildOptions();

        var gearUI = Object.FindAnyObjectByType<GearUI>();
        if (gearUI) gearUI.Refresh();
        foreach (var invUI in Object.FindObjectsByType<InventoryUI>(FindObjectsSortMode.None))
            invUI.Refresh();

        UpdateUI();
    }


    public void UpdateUI()
    {
        bool hasList = options.Count > 0 && index >= 0;
        if (leftBtn) leftBtn.interactable = hasList && index > 0;
        if (rightBtn) rightBtn.interactable = hasList && index < options.Count - 1;

        if (!hasList)
        {
            SetImage(centerIcon, equipped?.icon, Color.white);
            nameText.text = equipped != null ? equipped.itemName : "Nothing equipped";
            descText.text = equipped != null ? equipped.description : "";
            SetImage(leftPreviewIcon, null, previewHiddenColor);
            SetImage(rightPreviewIcon, null, previewHiddenColor);
            if (centerHighlight) centerHighlight.SetActive(equipped != null);
            RenderStats(equipped);
            return;
        }

        var sel = options[index];
        SetImage(centerIcon, sel.icon, Color.white);
        nameText.text = sel.itemName;
        descText.text = sel.description;

        if (index > 0)
            SetImage(leftPreviewIcon, options[index - 1].icon, previewDimColor);
        else
            SetImage(leftPreviewIcon, null, previewHiddenColor);

        if (index < options.Count - 1)
            SetImage(rightPreviewIcon, options[index + 1].icon, previewDimColor);
        else
            SetImage(rightPreviewIcon, null, previewHiddenColor);

        if (centerHighlight) centerHighlight.SetActive(equipped != null && ReferenceEquals(sel, equipped));

        RenderStats(sel);
    }

    private void SetImage(Image img, Sprite sprite, Color tint)
    {
        if (!img) return;
        img.sprite = sprite;
        img.color = tint;
        img.enabled = sprite != null || tint.a > 0f;
    }

    private void RenderStats(ItemData itemData)
    {
        foreach (var w in statPool) w.gameObject.SetActive(false);

        if (itemData is not IStatProvider provider || statRoot == null || statPrefab == null || statLibrary == null)
        {
            ForceRebuild();
            return;
        }

        var stats = provider.GetStats()
            .Select(s => (s, desc: statLibrary.Get(s.id)))
            .Where(t => !(t.desc.hideIfZero && Mathf.Approximately(t.s.value, 0)))
            .OrderBy(t => t.desc.priority)
            .ToList();

        foreach (var t in stats)
        {
            var w = GetStatWidget();
            var formatted = t.desc.Format(t.s.value) + (string.IsNullOrEmpty(t.desc.unit) ? "" : $" {t.desc.unit}");
            float? diff = null;
            w.Bind(t.desc.icon, formatted, diff, positiveColor, negativeColor);
        }

        ForceRebuild();
    }

    private StatWidget GetStatWidget()
    {
        var w = statPool.FirstOrDefault(x => !x.gameObject.activeSelf);
        if (w == null)
        {
            w = Instantiate(statPrefab, statRoot);
            statPool.Add(w);
        }
        return w;
    }

    private void ForceRebuild()
    {
        if (statRoot != null)
            LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)statRoot);
    }

    private void BuildOptions()
    {
        options = new List<GearData>();
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

    private void OnInventoryChanged()
    {
        RebuildAndRefresh();
    }

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


}
