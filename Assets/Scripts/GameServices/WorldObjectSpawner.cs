using UnityEngine;

public class WorldObjectSpawner : MonoBehaviour
{
    public static WorldObjectSpawner Instance { get; private set; }
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
    public SaveWorldItemsData CaptureAllWorldItems()
    {
        var data = new SaveWorldItemsData();

        // Getting all items on scene, filtering and capturing to data object
        var all = Object.FindObjectsByType<WorldItem>(FindObjectsSortMode.None);
        foreach (var wi in all)
        {
            if (wi == null || wi.data == null) continue;
            if (wi.amount <= 0) continue;

            data.items.Add(wi.Capture());
        }

        return data;
    }

    // Cleaning map from items before load
    public void ClearAllWorldItems()
    {
        var all = Object.FindObjectsByType<WorldItem>(FindObjectsSortMode.None);
        foreach (var wi in all)
            if (wi != null)
                Destroy(wi.gameObject);
    }

    // Loading items on map
    public void RestoreAllWorldItems(SaveWorldItemsData saved)
    {
        // If there is no save info we dont touch anything
        if (saved == null || saved.items == null || saved.items.Count == 0)
            return;

        // Cleaning
        ClearAllWorldItems();

        // Adding items on map
        foreach (var s in saved.items)
        {
            // Getting scriptable object by id from resolver (json can not serialize scriptableObjects)
            var itemData = ItemResolver.Resolve(s.itemId);
            if (itemData == null) continue;
  
            SpawnItem(itemData, s.amount, s.durability, s.position, s.rotation, Vector3.zero);
        }
    }


}
