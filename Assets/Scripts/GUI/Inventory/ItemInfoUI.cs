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

    [Header("Stats UI")]
    [SerializeField] private Transform statRoot;
    [SerializeField] private StatWidget statPrefab;
    [SerializeField] private StatLibrary statLibrary;
    [SerializeField] private Color positiveColor = Color.green;
    [SerializeField] private Color negativeColor = Color.red;

    readonly List<StatWidget> pool = new();

    public void SetItem(InventoryItem invItem, Inventory source)
    {
        var data = invItem.data;
        icon.sprite = data.icon;
        itemName.text = data.itemName;
        itemDescription.text = data.description;

        // очищаем пул
        foreach (var w in pool) w.gameObject.SetActive(false);

        // собираем статы
        var provider = data as IStatProvider;
        if (provider == null) return;

        var current = provider.GetStats()
            .Select(s => (s, desc: statLibrary.Get(s.id)))
            .Where(t => !(t.desc.hideIfZero && Mathf.Approximately(t.s.value, 0)))
            .OrderBy(t => t.desc.priority)
            .ToList();

        // база для сравнения (например, надето)
        var baseline = provider.GetBaselineForCompare().ToDictionary(x => x.id, x => x.value);

        foreach (var t in current)
        {
            var widget = Get();
            var formatted = t.desc.Format(t.s.value) + (string.IsNullOrEmpty(t.desc.unit) ? "" : $" {t.desc.unit}");
            float? diff = baseline.TryGetValue(t.s.id, out var oldVal) ? t.s.value - oldVal : (float?)null;
            widget.Bind(t.desc.icon, formatted, diff, positiveColor, negativeColor);
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)statRoot);
    }

    StatWidget Get()
    {
        var w = pool.FirstOrDefault(x => !x.gameObject.activeSelf);
        if (w == null) { w = Instantiate(statPrefab, statRoot); pool.Add(w); }
        return w;
    }
}