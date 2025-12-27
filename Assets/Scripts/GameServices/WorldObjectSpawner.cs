using UnityEngine;

public class WorldObjectSpawner : MonoBehaviour
{
    public static WorldObjectSpawner Instance { get; private set; }


    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public WorldItem SpawnItem(ItemData data, int amount, float currentDurability, Vector3 pos, Vector3 impulse)
    {
        if (data == null || amount <= 0) return null;

        GameObject prefab = data.pickupPrefab;
        if (!prefab)
        {
            Debug.LogError($"Item '{data.name}' has no pickupPrefab assigned!");
            return null;
        }

        var go = Instantiate(prefab, pos, Quaternion.identity);
        var wi = go.GetComponent<WorldItem>();
        if (!wi)
        {
            Debug.LogError($"pickupPrefab '{prefab.name}' has no WorldItem component!");
            return null;
        }

        wi.Init(data, amount, currentDurability);

        var rb = go.GetComponent<Rigidbody>();
        if (rb) rb.AddForce(impulse, ForceMode.Impulse);

        return wi;
    }

    // ===== SAVE =====

    public SaveWorldItemsData CaptureAllWorldItems()
    {
        var data = new SaveWorldItemsData();

        var all = Object.FindObjectsByType<WorldItem>(FindObjectsSortMode.None);
        foreach (var wi in all)
        {
            if (wi == null || wi.data == null) continue;
            if (wi.amount <= 0) continue;

            data.items.Add(wi.Capture());
        }

        return data;
    }

    public void ClearAllWorldItems()
    {
        var all = Object.FindObjectsByType<WorldItem>(FindObjectsSortMode.None);
        foreach (var wi in all)
            if (wi != null)
                Destroy(wi.gameObject);
    }

    public void RestoreAllWorldItems(SaveWorldItemsData saved)
    {
        if (saved == null || saved.items == null || saved.items.Count == 0)
            return;

        ClearAllWorldItems();

        foreach (var s in saved.items)
        {
            var itemData = ItemResolver.Resolve(s.itemId);
            if (itemData == null) continue;

            var wi = SpawnItem(itemData, s.amount, s.durability, s.position, Vector3.zero);
            if (wi != null) wi.transform.rotation = s.rotation;
        }
    }


}
