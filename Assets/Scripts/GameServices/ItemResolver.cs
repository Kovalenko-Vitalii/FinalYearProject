using System.Collections.Generic;
using UnityEngine;

public static class ItemResolver
{
    private static Dictionary<string, ItemData> _cache;

    private static void BuildCacheIfNeeded()
    {
        if (_cache != null) return;

        _cache = new Dictionary<string, ItemData>();

        var all = Resources.LoadAll<ItemData>("ItemSO");

        foreach (var it in all)
        {
            if (it == null) continue;

            if (string.IsNullOrWhiteSpace(it.id))
            {
                Debug.LogWarning($"[ItemResolver] ItemData '{it.name}' has EMPTY id");
                continue;
            }

            if (_cache.ContainsKey(it.id))
            {
                Debug.LogError($"[ItemResolver] DUPLICATE id '{it.id}' on '{it.name}'");
                continue;
            }

            _cache[it.id] = it;
        }
    }

    public static ItemData Resolve(string id)
    {
        if (string.IsNullOrWhiteSpace(id)) return null;

        BuildCacheIfNeeded();
        _cache.TryGetValue(id, out var found);
        return found;
    }

    public static void ClearCache() => _cache = null;
}
