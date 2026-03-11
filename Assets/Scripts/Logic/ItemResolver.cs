using System.Collections.Generic;
using UnityEngine;

// Static script used for retrieving items scriptable objects from id
// used during restoring items on map in WorldObjectSpawner
public static class ItemResolver
{
    private static Dictionary<string, ItemData> _cache;

    private static void BuildCacheIfNeeded()
    {
        if (_cache != null) return;

        // Id and scriptable object
        _cache = new Dictionary<string, ItemData>();

        // List of all scriptableObjects from resource folders
        var all = Resources.LoadAll<ItemData>("ItemSO");

        foreach (var it in all)
        {
            // Filtering all that does not have content, have no or wrong id
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

    // Get itemData (ScriptableObject) by id from dictionary
    public static ItemData Resolve(string id)
    {
        if (string.IsNullOrWhiteSpace(id)) return null;

        BuildCacheIfNeeded();
        _cache.TryGetValue(id, out var found);
        return found;
    }

    // Cleaning cache
    public static void ClearCache() => _cache = null;
}
