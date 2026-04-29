using UnityEngine;

// This class is responsible for spawning loot physically in the world or in continer
public class LootSpawnpoint : MonoBehaviour, ISaveable
{
    [Header("Save")]
    [SerializeField] private string id;
    [SerializeField] bool activated;

    [Header("Loot")]
    [SerializeField] ItemData[] lootPool;

    [Tooltip("Spawn settings")]
    [SerializeField, Min(1)] private int spawnAmount = 1;
    [SerializeField, Range(1, 100)] private int spawnChance = 100;

    [SerializeField] private bool randomAmount;
    [SerializeField] private bool randomDurability;

    WorldContainer container;

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

    void Awake() => container = GetComponent<WorldContainer>();
    
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

    private void SpawnLoot()
    {
        if (activated)
            return;

        activated = true;

        if (lootPool == null || lootPool.Length == 0)
            return;

        if (!RollSpawnChance())
            return;

        if (container != null)
        {
            SpawnIntoContainer();
        }
        else
        {
            SpawnInWorld();
        }
    }

    private bool RollSpawnChance()
    {
        return Random.Range(1, 101) <= spawnChance;
    }

    private void SpawnIntoContainer()
    {
        for (int i = 0; i < spawnAmount; i++)
        {
            if (!TryCreateLoot(out InventoryItem loot))
                continue;

            container.Inventory.AddItem(
                loot.data,
                loot.amount,
                loot.currentDurability
            );
        }
    }

    private void SpawnInWorld()
    {
        if (WorldObjectSpawner.Instance == null)
        {
            Debug.LogError("WorldObjectSpawner.Instance is missing", this);
            return;
        }

        if (!TryCreateLoot(out InventoryItem loot))
            return;

        WorldObjectSpawner.Instance.SpawnItem(
            loot,
            transform.position,
            Quaternion.identity,
            Vector3.zero
        );
    }

    private bool TryCreateLoot(out InventoryItem loot)
    {
        loot = default;

        ItemData candidate = GetRandomLootCandidate();

        if (candidate == null)
            return false;

        int amount = GetAmount(candidate);
        float durability = GetDurability(candidate);

        loot = new InventoryItem(candidate, amount, durability);
        return true;
    }

    private ItemData GetRandomLootCandidate()
    {
        for (int i = 0; i < lootPool.Length; i++)
        {
            ItemData candidate = lootPool[Random.Range(0, lootPool.Length)];

            if (candidate != null)
                return candidate;
        }

        return null;
    }

    private int GetAmount(ItemData item)
    {
        int maxAmount = Mathf.Max(1, item.maxStack);

        if (randomAmount)
            return Random.Range(1, maxAmount + 1);

        return maxAmount;
    }

    private float GetDurability(ItemData item)
    {
        if (!item.hasDurability)
            return 0f;

        if (randomDurability)
            return Random.Range(1f, item.maxDurability);

        return item.maxDurability;
    }

    // Save/Load
    public object CaptureState()
    {
        return new LootSpawnpointSave { activated = this.activated};
    }

    public void RestoreState(object state)
    {
        if (state is not LootSpawnpointSave s) return;
        activated = s.activated;
    }

    public void ResetToDefaultState()
    {

    }
}

public struct LootSpawnpointSave
{
    public bool activated;
}