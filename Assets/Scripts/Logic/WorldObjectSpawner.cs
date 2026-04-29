using System;
using System.Collections.Generic;
using UnityEngine;

public class WorldObjectSpawner : MonoBehaviour, ISaveable
{
    public static WorldObjectSpawner Instance { get; private set; }

    public string SaveId => "WORLD_OBJECT_SPAWNER";
    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    // Method used for spawning items on scene
    public WorldItem SpawnItem(InventoryItem item, Vector3 pos, Quaternion rotation, Vector3 impulse)
    {
        // Checking if we have itemData at all
        if (item == null || item.data == null || item.amount <= 0)
            return null;

        // Getting spawn prefab for item
        GameObject prefab = item.data.pickupPrefab;
        if (!prefab)
        {
            Debug.LogError($"Item '{item.data.name}' has no pickupPrefab assigned!");
            return null;
        }

        // Instantiating prefab and giving it a postionion
        var go = Instantiate(prefab, pos, rotation);
        // Getting WorldItem component and setting up dynamic values
        var wi = go.GetComponent<WorldItem>();
        if (!wi)
        {
            Debug.LogError($"pickupPrefab '{prefab.name}' has no WorldItem component!");
            Destroy(go);
            return null;
        }

        wi.Init(item);

        var rb = go.GetComponent<Rigidbody>();
        if (rb)
            rb.AddForce(impulse, ForceMode.Impulse);

        return wi;
    }

    // ===== SAVE =====

    // Capturing data about all items on location
    public object CaptureState()
    {
        var data = new SaveWorldItemsData();

        // Getting all items on scene, filtering and capturing to data object
        var all = FindObjectsByType<WorldItem>(FindObjectsSortMode.None);

        foreach (var wi in all)
        {
            if (wi == null || wi.Item == null || wi.Data == null)
                continue;

            if (wi.Amount <= 0)
                continue;

            data.items.Add(wi.Capture());
        }

        return data;
    }

    // Loading items on map
    public void RestoreState(object state)
    {
        var saved = state as SaveWorldItemsData;

        // Cleaning map first
        var all = FindObjectsByType<WorldItem>(FindObjectsSortMode.None);
        foreach (var wi in all)
        {
            if (wi != null)
                Destroy(wi.gameObject);
        }

        // If there is no save info we dont touch anything
        if (saved == null || saved.items == null || saved.items.Count == 0)
            return;

        // Adding items on map
        foreach (var s in saved.items)
        {
            InventoryItem item = s.item.ToRuntime();
            if (item == null)
            {
                Debug.LogWarning("WorldObjectSpawner: failed to restore world item");
                continue;
            }

            SpawnItem(item, s.position, s.rotation, Vector3.zero);
        }
    }

    public void ResetToDefaultState()
    {
        
    }
}

// Saving items on location
[Serializable]
public class SaveWorldItemsData
{
    public List<WorldItemSave> items = new();
}

// This could be inherited from InventoryItemSave
[Serializable]
public struct WorldItemSave
{
    public InventoryItemSave item;
    public Vector3 position;
    public Quaternion rotation;
}
