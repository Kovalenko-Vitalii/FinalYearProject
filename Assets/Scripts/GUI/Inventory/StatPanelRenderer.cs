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
