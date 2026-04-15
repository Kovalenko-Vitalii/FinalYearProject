using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class StatPanelRenderer : MonoBehaviour
{
    [SerializeField] private Transform statRoot;
    [SerializeField] private StatWidget statPrefab;
    [SerializeField] private StatLibrary statLibrary;
    [SerializeField] private Color positiveColor = Color.green;
    [SerializeField] private Color negativeColor = Color.red;

    private readonly List<StatWidget> pool = new();

    public void Render(ItemData itemData, IEnumerable<StatValue> baseline = null)
    {
        foreach (var w in pool) w.gameObject.SetActive(false);

        if (itemData is not IStatProvider provider || statRoot == null || statPrefab == null || statLibrary == null)
        {
            ForceRebuild();
            return;
        }

        var current = provider.GetStats()
            .Select(s => (s, desc: statLibrary.Get(s.id)))
            .Where(t => !(t.desc.hideIfZero && Mathf.Approximately(t.s.value, 0)))
            .OrderBy(t => t.desc.priority)
            .ToList();

        var baseDict = (baseline ?? provider.GetBaselineForCompare()).ToDictionary(x => x.id, x => x.value);

        foreach (var t in current)
        {
            var widget = Get();
            var formatted = t.desc.Format(t.s.value) + (string.IsNullOrEmpty(t.desc.unit) ? "" : $" {t.desc.unit}");
            float? diff = baseDict.TryGetValue(t.s.id, out var oldVal) ? t.s.value - oldVal : (float?)null;
            widget.Bind(t.desc.icon, formatted, diff, positiveColor, negativeColor);
        }

        ForceRebuild();
    }

    public void Render(InventoryItem invItem)
    {
        foreach (var w in pool) w.gameObject.SetActive(false);

        if (invItem == null || invItem.data is not IStatProvider provider ||
            statRoot == null || statPrefab == null || statLibrary == null)
        {
            ForceRebuild();
            return;
        }

        var data = invItem.data;

        var stats = provider.GetStats().ToList();

        if (data.hasDurability && data.maxDurability > 0f)
        {
            float ratio = Mathf.Clamp01(invItem.currentDurability / data.maxDurability);
            float percent = ratio * 100f;

            stats.Add(new StatValue
            {
                id = StatId.Durability,
                value = percent
            });
        }

        var current = stats
            .Select(s => (s, desc: statLibrary.Get(s.id)))
            .Where(t => !(t.desc.hideIfZero && Mathf.Approximately(t.s.value, 0)))
            .OrderBy(t => t.desc.priority)
            .ToList();

        var baseDict = new Dictionary<StatId, float>();

        foreach (var t in current)
        {
            var widget = Get();
            string formatted;

            if (t.s.id == StatId.Durability && data.hasDurability && data.maxDurability > 0f)
            {
                int currentDurability = Mathf.CeilToInt(invItem.currentDurability);
                int maxDurability = Mathf.CeilToInt(data.maxDurability);
                formatted = $"{currentDurability}/{maxDurability}";
            }
            else
            {
                formatted = t.desc.Format(t.s.value) + (string.IsNullOrEmpty(t.desc.unit) ? "" : $" {t.desc.unit}");
            }

            float? diff = baseDict.TryGetValue(t.s.id, out var oldVal) ? t.s.value - oldVal : (float?)null;
            widget.Bind(t.desc.icon, formatted, diff, positiveColor, negativeColor);
        }

        ForceRebuild();
    }

    private StatWidget Get()
    {
        var w = pool.FirstOrDefault(x => !x.gameObject.activeSelf);
        if (w == null) { w = Instantiate(statPrefab, statRoot); pool.Add(w); }
        w.gameObject.SetActive(true);
        return w;
    }

    private void ForceRebuild()
    {
        if (statRoot != null)
            LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)statRoot);
    }

    public void Clear()
    {
        foreach (var w in pool)
            w.gameObject.SetActive(false);

        ForceRebuild();
    }
}
