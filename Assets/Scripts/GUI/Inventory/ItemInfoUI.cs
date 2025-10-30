using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemInfoUI : MonoBehaviour
{
    [Header("Header UI")]
    [SerializeField] private Image icon;
    [SerializeField] private TextMeshProUGUI itemName;
    [SerializeField] private TextMeshProUGUI itemDescription;
    [SerializeField] private Button buttonEquip;
    [SerializeField] private Button buttonDelete;

    [Header("Stats UI")]
    [SerializeField] private Transform statRoot;
    [SerializeField] private StatWidget statPrefab;
    [SerializeField] private StatLibrary statLibrary;
    [SerializeField] private Color positiveColor = Color.green;
    [SerializeField] private Color negativeColor = Color.red;

    [Header("Defaults")]
    [SerializeField] private Sprite defaultIcon;
    [SerializeField] private string defaultName = "";
    [SerializeField] [TextArea] private string defaultDescription = "";

    readonly List<StatWidget> pool = new();

    private Inventory trackedSource;
    private ItemData trackedItem;
    private int trackedCount;

    private bool subscribed;

    void Awake()
    {
        ShowDefault();
    }

    public void SetItem(InventoryItem invItem, Inventory source)
    {
        if (invItem == null || invItem.data == null)
        {
            ShowDefault();
            return;
        }

        trackedSource = source;
        trackedItem = invItem.data;
        trackedCount = CountInInventory(trackedSource, trackedItem);

        var data = invItem.data;
        icon.sprite = data.icon != null ? data.icon : defaultIcon;
        itemName.text = string.IsNullOrEmpty(data.itemName) ? defaultName : data.itemName;
        itemDescription.text = string.IsNullOrEmpty(data.description) ? defaultDescription : data.description;

        if (buttonEquip) buttonEquip.gameObject.SetActive(true);
        if (buttonDelete) buttonDelete.gameObject.SetActive(true);

        foreach (var w in pool) w.gameObject.SetActive(false);

        var provider = data as IStatProvider;
        if (provider != null)
        {
            var current = provider.GetStats()
                .Select(s => (s, desc: statLibrary.Get(s.id)))
                .Where(t => !(t.desc.hideIfZero && Mathf.Approximately(t.s.value, 0)))
                .OrderBy(t => t.desc.priority)
                .ToList();

            var baseline = provider.GetBaselineForCompare().ToDictionary(x => x.id, x => x.value);

            foreach (var t in current)
            {
                var widget = Get();
                var formatted = t.desc.Format(t.s.value) + (string.IsNullOrEmpty(t.desc.unit) ? "" : $" {t.desc.unit}");
                float? diff = baseline.TryGetValue(t.s.id, out var oldVal) ? t.s.value - oldVal : (float?)null;
                widget.Bind(t.desc.icon, formatted, diff, positiveColor, negativeColor);
            }
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)statRoot);

        if (buttonDelete != null)
        {
            buttonDelete.onClick.RemoveAllListeners();
            buttonDelete.onClick.AddListener(() =>
            {
                source.RemoveItem(invItem.data, 1);

                var ui = Object.FindAnyObjectByType<InventoryUI>();
                if (ui != null) ui.Refresh();

                var gearUI = Object.FindAnyObjectByType<GearUI>();
                if (gearUI != null) gearUI.Refresh();

                ShowDefault();
            });
        }

        if (buttonEquip != null)
        {
            buttonEquip.onClick.RemoveAllListeners();
            buttonEquip.onClick.AddListener(() =>
            {
                var im = InventoryManager.Instance;
                if (trackedItem is GearData gearData)
                {
                    var oldGear = im.playerEquipment.Equip(gearData);
                    source.RemoveItem(trackedItem, 1);
                    if (oldGear != null) source.AddItem(oldGear, 1);

                    var gearUI = Object.FindAnyObjectByType<GearUI>();
                    if (gearUI != null) gearUI.Refresh();

                    foreach (var invUI in Object.FindObjectsByType<InventoryUI>(FindObjectsSortMode.None))
                        invUI.Refresh();

                    trackedCount = CountInInventory(trackedSource, trackedItem);
                    UpdateEquipButtonState();
                    if (trackedCount == 0) ShowDefault();
                }
            });
        }

        UpdateEquipButtonState();
        Subscribe();
    }

    public void ShowDefault()
    {
        if (icon) icon.sprite = defaultIcon;
        if (itemName) itemName.text = defaultName;
        if (itemDescription) itemDescription.text = defaultDescription;

        foreach (var w in pool) w.gameObject.SetActive(false);
        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)statRoot);

        if (buttonEquip) buttonEquip.gameObject.SetActive(false);
        if (buttonDelete) buttonDelete.gameObject.SetActive(false);

        trackedSource = null;
        trackedItem = null;
        trackedCount = 0;
    }

    StatWidget Get()
    {
        var w = pool.FirstOrDefault(x => !x.gameObject.activeSelf);
        if (w == null) { w = Instantiate(statPrefab, statRoot); pool.Add(w); }
        w.gameObject.SetActive(true);
        return w;
    }

    private static int CountInInventory(Inventory inv, ItemData data)
       => inv?.items.Where(i => i.data == data).Sum(i => i.amount) ?? 0;

    private static bool IsInInventory(Inventory inv, ItemData data)
        => CountInInventory(inv, data) > 0;

    private void Subscribe()
    {
        if (subscribed) return;
        var im = InventoryManager.Instance;
        if (im != null)
        {
            im.OnPlayerInventoryChanged += OnInventoryChanged;
            if (im.playerEquipment != null)
                im.playerEquipment.OnChanged += OnEquipmentChanged;
            subscribed = true;
        }
    }

    private void Unsubscribe()
    {
        if (!subscribed) return;
        var im = InventoryManager.Instance;
        if (im != null)
        {
            im.OnPlayerInventoryChanged -= OnInventoryChanged;
            if (im.playerEquipment != null)
                im.playerEquipment.OnChanged -= OnEquipmentChanged;
        }
        subscribed = false;
    }

    private void OnDestroy() => Unsubscribe();

    private void OnInventoryChanged()
    {
        if (trackedSource == null || trackedItem == null) return;

        int current = CountInInventory(trackedSource, trackedItem);

        if (current == 0)
        {
            ShowDefault();
            return;
        }

        if (current != trackedCount)
        {
            trackedCount = current;
            UpdateEquipButtonState();
        }
    }

    private void OnEquipmentChanged(GearData.GearSlot slot, GearData oldGear, GearData newGear)
    {
        if (trackedItem is not GearData g) return;
        if (g.slot != slot) return;

        if (ReferenceEquals(newGear, trackedItem) && !IsInInventory(trackedSource, trackedItem))
        {
            ShowDefault();
            return;
        }

        trackedCount = CountInInventory(trackedSource, trackedItem);
        UpdateEquipButtonState();
    }

    private void UpdateEquipButtonState()
    {
        if (buttonEquip == null) return;

        if (trackedItem is GearData g)
        {
            var eq = InventoryManager.Instance.playerEquipment.GetEquipped(g.slot);
            bool canEquip = !ReferenceEquals(eq, trackedItem) && trackedCount > 0;
            buttonEquip.interactable = canEquip;
        }
        else
        {
            buttonEquip.gameObject.SetActive(false);
        }
    }
}
