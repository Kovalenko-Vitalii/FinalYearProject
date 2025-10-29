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
                var inventoryManager = InventoryManager.Instance;

                if (invItem.data is GearData gearData)
                {
                    var oldGear = inventoryManager.playerEquipment.Equip(gearData);

                    source.RemoveItem(invItem.data, 1);
                    if (oldGear != null) source.AddItem(oldGear, 1);

                    var gearUI = Object.FindAnyObjectByType<GearUI>();
                    if (gearUI != null) gearUI.Refresh();

                    foreach (var invUI in Object.FindObjectsByType<InventoryUI>(FindObjectsSortMode.None))
                        invUI.Refresh();
                    ShowDefault();
                }
            });
        }
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
    }

    StatWidget Get()
    {
        var w = pool.FirstOrDefault(x => !x.gameObject.activeSelf);
        if (w == null) { w = Instantiate(statPrefab, statRoot); pool.Add(w); }
        w.gameObject.SetActive(true);
        return w;
    }
}
