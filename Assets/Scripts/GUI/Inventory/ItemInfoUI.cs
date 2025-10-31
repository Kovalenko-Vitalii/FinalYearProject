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
    [SerializeField] private Button buttonActions;

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
        if (buttonActions) buttonActions.gameObject.SetActive(true);

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

        BindFixedButtons(invItem, source);

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

            var item = trackedSource.items.FirstOrDefault(i => i.data == trackedItem);
            if (item != null) BindFixedButtons(item, trackedSource);
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

        var item = trackedSource.items.FirstOrDefault(i => i.data == trackedItem);
        if (item != null) BindFixedButtons(item, trackedSource);
    }

    private void BindFixedButtons(InventoryItem invItem, Inventory source)
    {
        if (buttonDelete) { buttonDelete.onClick.RemoveAllListeners(); buttonDelete.gameObject.SetActive(false); }
        if (buttonEquip) { buttonEquip.onClick.RemoveAllListeners(); buttonEquip.gameObject.SetActive(false); }
        if (buttonActions) { buttonActions.onClick.RemoveAllListeners(); buttonActions.gameObject.SetActive(false); }

        if (invItem.data is not IItemActionProvider provider) return;

        var ctx = new ItemActionContext { source = source, item = invItem, equipment = InventoryManager.Instance.playerEquipment };
        var actions = provider.GetActions(ctx).ToList();

        var drop = actions.Where(a => a.slot == ActionSlot.Drop).FirstOrDefault();
        if (buttonDelete && drop.execute != null)
        {
            buttonDelete.gameObject.SetActive(true);
            buttonDelete.interactable = drop.interactable;
            buttonDelete.onClick.AddListener(() => { drop.execute?.Invoke(); AfterActionRefresh(); });
        }

        var primary = actions.Where(a => a.slot == ActionSlot.Use).FirstOrDefault();
        if (buttonEquip && primary.execute != null)
        {
            buttonEquip.gameObject.SetActive(true);
            buttonEquip.interactable = primary.interactable;

            var label = buttonEquip.GetComponentInChildren<TMPro.TextMeshProUGUI>(true);
            if (label) label.text = string.IsNullOrEmpty(primary.label) ? "Use" : primary.label;

            buttonEquip.onClick.AddListener(() => { primary.execute?.Invoke(); AfterActionRefresh(); });
        }
    }


    private void AfterActionRefresh()
    {
        if (trackedSource == null || trackedItem == null) { ShowDefault(); return; }

        trackedCount = CountInInventory(trackedSource, trackedItem);
        if (trackedCount == 0)
        {
            ShowDefault();
            return;
        }

        var item = trackedSource.items.FirstOrDefault(i => i.data == trackedItem);
        if (item != null) BindFixedButtons(item, trackedSource);
    }
}
