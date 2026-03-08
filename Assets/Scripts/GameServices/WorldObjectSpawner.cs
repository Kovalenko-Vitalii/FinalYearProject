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
    public WorldItem SpawnItem(ItemData data, int amount, float currentDurability, Vector3 pos, Quaternion rotation, Vector3 impulse)
    {
        // Checking if we have itemData at all
        if (data == null || amount <= 0) return null;

        // Getting spawn prefab for item
        GameObject prefab = data.pickupPrefab;
        if (!prefab)
        {
            Debug.LogError($"Item '{data.name}' has no pickupPrefab assigned!");
            return null;
        }

        // Instantiating prefab and giving it a postionion
        var go = Instantiate(prefab, pos, rotation);
        // Getting WorldItem component and setting up dynamic values
        var wi = go.GetComponent<WorldItem>();
        if (!wi)
        {
            Debug.LogError($"pickupPrefab '{prefab.name}' has no WorldItem component!");
            return null;
        }

        wi.Init(data, amount, currentDurability);

        // Applying impulse
        var rb = go.GetComponent<Rigidbody>();
        if (rb) rb.AddForce(impulse, ForceMode.Impulse);

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
            if (wi == null || wi.data == null) continue;
            if (wi.amount <= 0) continue;

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
            // Getting scriptable object by id from resolver (json can not serialize scriptableObjects)
            var itemData = ItemResolver.Resolve(s.itemId);
            if (itemData == null)
            {
                Debug.LogWarning($"WorldObjectSpawner: failed to resolve itemId '{s.itemId}'");
                continue;
            }

            SpawnItem(
                itemData,
                s.amount,
                s.durability,
                s.position,
                s.rotation,
                Vector3.zero
            );
        }
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
    public string itemId;
    public Vector3 position;
    public Quaternion rotation;
    public int amount;
    public float durability;
}
