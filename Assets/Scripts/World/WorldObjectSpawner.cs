using UnityEngine;

public class WorldObjectSpawner : MonoBehaviour
{
    public static WorldObjectSpawner Instance { get; private set; }


    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public WorldItem SpawnItem(ItemData data, int amount, Vector3 pos, Vector3 impulse)
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

        wi.Init(data, amount);

        var rb = go.GetComponent<Rigidbody>();
        if (rb) rb.AddForce(impulse, ForceMode.Impulse);

        return wi;
    }

}
