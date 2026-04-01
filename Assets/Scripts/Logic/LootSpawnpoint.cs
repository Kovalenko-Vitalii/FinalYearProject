using UnityEngine;

public class LootSpawnpoint : MonoBehaviour, ISaveable
{
    [Header("Save")]
    [SerializeField] private string id;

    [SerializeField] bool activated;
    [SerializeField] ItemData[] lootPool;
    [SerializeField] bool random; // If selected - random durability and amount else - max durability and amount

    public string SaveId => id;


    private void Reset()
    {
#if UNITY_EDITOR
        SaveIdUtil.EnsureId(ref id, this);
#else
            if (string.IsNullOrWhiteSpace(id))
                id = System.Guid.NewGuid().ToString("N");
#endif
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        SaveIdUtil.EnsureId(ref id, this);
    }
#endif

    private void OnEnable()
    {
        if (GameplayOrchestrator.Instance != null)
            GameplayOrchestrator.Instance.OnFreshGameplayStarted += HandleFreshGameplayStarted;
    }

    private void OnDisable()
    {
        if (GameplayOrchestrator.Instance != null)
            GameplayOrchestrator.Instance.OnFreshGameplayStarted -= HandleFreshGameplayStarted;
    }

    private void HandleFreshGameplayStarted()
    {
        SpawnLoot();
    }

    void SpawnLoot()
    {
        if (activated)
            return;

        if (lootPool == null || lootPool.Length == 0)
            return;

        if (WorldObjectSpawner.Instance == null)
        {
            Debug.LogError("WorldObjectSpawner.Instance is missing", this);
            return;
        }

        var candidate = lootPool[Random.Range(0, lootPool.Length)];
        if (candidate == null)
            return;

        int amount;
        float durability;

        if (random)
        {
            amount = Mathf.Max(1, Random.Range(1, candidate.maxStack + 1));

            if (candidate.hasDurability)
                durability = Random.Range(1f, candidate.maxDurability);
            else
                durability = 0f;
        }
        else
        {
            amount = Mathf.Max(1, candidate.maxStack);
            durability = candidate.hasDurability ? candidate.maxDurability : 0f;
        }

        var loot = new InventoryItem(candidate, amount, durability);

        WorldObjectSpawner.Instance.SpawnItem(
            loot,
            transform.position,
            Quaternion.identity,
            Vector3.zero
        );

        activated = true;
    }

    public object CaptureState()
    {
        return new LootSpawnpointSave { activated = this.activated};
    }

    public void RestoreState(object state)
    {
        if (state is not LootSpawnpointSave s) return;
        activated = s.activated;
    }
}

public struct LootSpawnpointSave
{
    public bool activated;
}