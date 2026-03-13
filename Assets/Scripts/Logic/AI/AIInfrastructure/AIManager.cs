using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages AI lifecycle:
/// - keeps runtime registry of spawned creatures
/// - controls respawn timers
/// - handles death / corpse despawn
/// - supports save / load
/// </summary>
public class AIManager : MonoBehaviour, ISaveable, IPlayerTick
{
    private const string TAG = "AI_MANAGER";
    public string SaveId => TAG;

    public static AIManager Instance { get; private set; }


    [SerializeField] private float corpseLifetime = 10f;
    [SerializeField] private List<AISpawnConfig> aiSpawnConfigs = new();


    // Cache
    readonly List<AISpawnConfig> validSpawnConfigs = new(); // Runtime-validated configs only
    readonly Dictionary<string, AISpawnConfig> spawnConfigById = new(); // Fast config lookup by creature id

    // Spawn point system
    List<AISpawnPoint> aiSpawnPoints = new(); // All registered spawn points in the scene
    Dictionary<string, List<AISpawnPoint>> spawnPointsByAiId = new(); // Cached spawn points grouped by creature id
  
    // Respawn data
    Dictionary<string, float> respawnTimers = new(); // Respawn timers by creature id
    Dictionary<string, int> aliveCounts = new(); // Cached alive counters by creature id

    // AI runtime registry
    List<AIRuntimeEntry> activeAIs = new(); // Main runtime registry of all spawned AIs
    Dictionary<AgentBrain, AIRuntimeEntry> entryByBrain = new(); // Fast lookup from brain to runtime entry

    [Serializable]
    public class AIRuntimeEntry
    {
        public string aiId;
        public AgentBrain brain;
      
        public bool isDead; // True after death was reported

        public float corpseDespawnTime = -1f; // Time when corpse should be removed
    }

    #region Unity lifecycle

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        BuildConfigMap();
    }

    private void Start() => PlayerTickSystem.Instance?.Register(this);
    private void OnDisable() => PlayerTickSystem.Instance?.Unregister(this);
           
    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    #endregion

    #region Main tick

    /// <summary>
    /// Main manager tick.
    /// Performs runtime AI cleanup / corpse despawn
    /// and then updates spawn timers.
    /// </summary>
    public void Tick(float dt)
    {
        TickAIRegistry();
        TickSpawns(dt);
    }

    #endregion

    #region Spawn point registry

    /// <summary>
    /// Registers a spawn point and adds it to cached lookup tables.
    /// </summary>
    public void RegisterAISpawnPoint(AISpawnPoint point)
    {
        if (point == null)
            return;

        if (aiSpawnPoints.Contains(point))
            return;

        aiSpawnPoints.Add(point);
        AddSpawnPointToMap(point);
    }

    /// <summary>
    /// Unregisters a spawn point and removes it from cached lookup tables.
    /// </summary> 
    public void UnregisterAISpawnPoint(AISpawnPoint point)
    {
        if (point == null)
            return;

        aiSpawnPoints.Remove(point);
        RemoveSpawnPointFromMap(point);
    }

    /// <summary>
    /// Rebuilds all spawn point caches from the current registry.
    /// Useful after config rebuild or scene changes.
    /// </summary>
    private void RebuildSpawnPointMap()
    {
        foreach (var pair in spawnPointsByAiId)
            pair.Value.Clear();

        for (int i = aiSpawnPoints.Count - 1; i >= 0; i--)
        {
            var point = aiSpawnPoints[i];
            if (point == null)
            {
                aiSpawnPoints.RemoveAt(i);
                continue;
            }

            AddSpawnPointToMap(point);
        }
    }

    /// <summary>
    /// Adds a spawn point into all per-creature cached lists it supports.
    /// </summary>
    private void AddSpawnPointToMap(AISpawnPoint point)
    {
        if (point == null || point.AllowedCreatureIds == null)
            return;

        var ids = point.AllowedCreatureIds;
        for (int i = 0; i < ids.Length; i++)
        {
            string aiId = ids[i];
            if (string.IsNullOrWhiteSpace(aiId))
                continue;

            if (!spawnPointsByAiId.TryGetValue(aiId, out var list))
            {
                list = new List<AISpawnPoint>();
                spawnPointsByAiId[aiId] = list;
            }

            if (!list.Contains(point))
                list.Add(point);
        }
    }

    /// <summary>
    /// Removes a spawn point from all cached per-creature lists it belongs to.
    /// </summary>
    private void RemoveSpawnPointFromMap(AISpawnPoint point)
    {
        if (point == null || point.AllowedCreatureIds == null)
            return;

        var ids = point.AllowedCreatureIds;
        for (int i = 0; i < ids.Length; i++)
        {
            string aiId = ids[i];
            if (string.IsNullOrWhiteSpace(aiId))
                continue;

            if (spawnPointsByAiId.TryGetValue(aiId, out var list))
                list.Remove(point);
        }
    }

    #endregion

    #region AI registry

    /// <summary>
    /// Adds a newly spawned AI into runtime registry.
    /// </summary>
    private void RegisterAI(string aiId, AgentBrain brain)
    {
        if (brain == null || string.IsNullOrWhiteSpace(aiId))
            return;

        if (entryByBrain.ContainsKey(brain))
            return;

        var entry = new AIRuntimeEntry
        {
            aiId = aiId,
            brain = brain,
            isDead = false,
            corpseDespawnTime = -1f
        };

        activeAIs.Add(entry);
        entryByBrain.Add(brain, entry);
        AddAliveCount(aiId, 1);
    }

    /// <summary>
    /// Removes AI from runtime registry.
    /// </summary>
    private void UnregisterAI(AgentBrain brain)
    {
        if (brain == null)
            return;

        if (!entryByBrain.TryGetValue(brain, out var entry))
            return;

        if (!entry.isDead && !string.IsNullOrWhiteSpace(entry.aiId))
            AddAliveCount(entry.aiId, -1);

        entryByBrain.Remove(brain);
        activeAIs.Remove(entry);
    }

    /// <summary>
    /// Single-pass runtime maintenance:
    /// - removes invalid entries
    /// - removes destroyed brains
    /// - despawns expired corpses
    /// </summary>
    private void TickAIRegistry()
    {
        float now = Time.time;

        for (int i = activeAIs.Count - 1; i >= 0; i--)
        {
            var entry = activeAIs[i];

            if (entry == null)
            {
                activeAIs.RemoveAt(i);
                continue;
            }

            var brain = entry.brain;

            if (brain == null)
            {
                if (!entry.isDead && !string.IsNullOrWhiteSpace(entry.aiId))
                    AddAliveCount(entry.aiId, -1);

                activeAIs.RemoveAt(i);
                continue;
            }

            if (!entry.isDead)
                continue;

            if (entry.corpseDespawnTime < 0f || now < entry.corpseDespawnTime)
                continue;

            entryByBrain.Remove(brain);
            Destroy(brain.gameObject);
            activeAIs.RemoveAt(i);
        }
    }

    #endregion

    #region Configs

    /// <summary>
    /// Validates spawn configs and rebuilds runtime caches.
    /// </summary>
    private void BuildConfigMap()
    {
        spawnConfigById.Clear();
        validSpawnConfigs.Clear();
        aliveCounts.Clear();
        respawnTimers.Clear();

        // Keep existing spawn-point dictionary keys if we want,
        // but easiest and safest is to rebuild it fully.
        spawnPointsByAiId.Clear();

        foreach (var config in aiSpawnConfigs)
        {
            if (config == null)
                continue;

            if (string.IsNullOrWhiteSpace(config.creatureId))
            {
                GameLog.Warning(TAG, "Found spawn config with empty creatureId");
                continue;
            }

            if (spawnConfigById.ContainsKey(config.creatureId))
            {
                GameLog.Warning(TAG, $"Duplicate creatureId '{config.creatureId}'");
                continue;
            }

            spawnConfigById.Add(config.creatureId, config);
            validSpawnConfigs.Add(config);

            respawnTimers[config.creatureId] = config.respawnDelay;
            aliveCounts[config.creatureId] = 0;
            spawnPointsByAiId[config.creatureId] = new List<AISpawnPoint>();
        }

        RebuildSpawnPointMap();
    }

    #endregion

    #region Spawn logic

    /// <summary>
    /// Updates respawn timers and attempts to spawn missing AI population.
    /// </summary>
    private void TickSpawns(float dt)
    {
        for (int i = 0; i < validSpawnConfigs.Count; i++)
        {
            var config = validSpawnConfigs[i];
            string aiId = config.creatureId;

            respawnTimers.TryGetValue(aiId, out float timer);

            int alive = GetAliveCount(aiId);

            // Population cap reached: keep timer reset to full delay
            if (alive >= config.maxAliveCount)
            {
                respawnTimers[aiId] = config.respawnDelay;
                continue;
            }

            timer -= dt;

            if (timer > 0f)
            {
                respawnTimers[aiId] = timer;
                continue;
            }

            bool spawned = TrySpawnAI(aiId);
            respawnTimers[aiId] = config.respawnDelay;

            if (!spawned)
                GameLog.Warning(TAG, $"Failed to spawn '{aiId}'");
        }
    }

    /// <summary>
    /// Tries to spawn one AI instance by id.
    /// </summary>
    public bool TrySpawnAI(string aiId)
    {
        if (string.IsNullOrWhiteSpace(aiId))
        {
            GameLog.Warning(TAG, "TrySpawnAI failed: aiId is null or empty");
            return false;
        }

        if (!spawnConfigById.TryGetValue(aiId, out var config))
        {
            GameLog.Warning(TAG, $"No config found for '{aiId}'");
            return false;
        }

        if (config.prefab == null)
        {
            GameLog.Warning(TAG, $"Config '{aiId}' has no prefab");
            return false;
        }

        int alive = GetAliveCount(aiId);
        if (alive >= config.maxAliveCount)
        {
            GameLog.Warning(TAG, $"Spawn blocked for '{aiId}', alive={alive}, max={config.maxAliveCount}");
            return false;
        }

        var point = PickRandomSpawnPoint(aiId);
        if (point == null)
        {
            GameLog.Warning(TAG, $"No spawn point found for '{aiId}'");
            return false;
        }

        GameObject go = Instantiate(config.prefab, point.Position, Quaternion.identity);
        if (go == null)
        {
            GameLog.Warning(TAG, $"Instantiate failed for '{aiId}'");
            return false;
        }

        AgentBrain brain = go.GetComponent<AgentBrain>();
        if (brain == null)
        {
            Debug.LogError($"AIManager: prefab '{config.prefab.name}' has no AgentBrain component", go);
            Destroy(go);
            return false;
        }

        RegisterAI(aiId, brain);
        GameLog.Log(TAG, $"Spawned '{aiId}' at {point.Position}");
        return true;
    }

    /// <summary>
    /// Picks a random cached spawn point for the given AI id.
    /// </summary>
    private AISpawnPoint PickRandomSpawnPoint(string aiId)
    {
        if (string.IsNullOrWhiteSpace(aiId))
            return null;

        if (!spawnPointsByAiId.TryGetValue(aiId, out var points) || points == null || points.Count == 0)
            return null;

        // Lazy cleanup of destroyed spawn points in this specific bucket
        for (int i = points.Count - 1; i >= 0; i--)
        {
            if (points[i] != null)
                continue;

            points.RemoveAt(i);
        }

        if (points.Count == 0)
            return null;

        int index = UnityEngine.Random.Range(0, points.Count);
        return points[index];
    }

    /// <summary>
    /// Returns alive count for a creature id.
    /// </summary>
    private int GetAliveCount(string aiId)
    {
        if (string.IsNullOrWhiteSpace(aiId))
            return 0;

        return aliveCounts.TryGetValue(aiId, out int count) ? count : 0;
    }

    /// <summary>
    /// Adds delta to cached alive count and clamps result to zero.
    /// </summary>
    private void AddAliveCount(string aiId, int delta)
    {
        if (string.IsNullOrWhiteSpace(aiId))
            return;

        aliveCounts.TryGetValue(aiId, out int count);
        count += delta;

        if (count < 0)
            count = 0;

        aliveCounts[aiId] = count;
    }

    #endregion

    #region Death / corpse despawn

    /// <summary>
    /// Marks AI as dead and schedules corpse despawn.
    /// Must be called by external death logic.
    /// </summary>
    public void NotifyAIDied(AgentBrain brain)
    {
        if (brain == null)
            return;

        if (!entryByBrain.TryGetValue(brain, out var entry))
            return;

        if (entry.isDead)
            return;

        entry.isDead = true;
        entry.corpseDespawnTime = Time.time + corpseLifetime;

        AddAliveCount(entry.aiId, -1);
    }

    #endregion

    #region Manual cleanup

    /// <summary>
    /// Destroys all registered AI game objects and resets runtime counters.
    /// </summary>
    public void ClearAllAIs()
    {
        foreach (var entry in activeAIs)
        {
            if (entry != null && entry.brain != null)
                Destroy(entry.brain.gameObject);
        }

        activeAIs.Clear();
        entryByBrain.Clear();

        var keys = new List<string>(aliveCounts.Keys);
        foreach (var key in keys)
            aliveCounts[key] = 0;
    }

    #endregion

    #region Save / Load

    public object CaptureState()
    {
        var data = new SaveAIData();

        // Make sure registry is clean before serializing
        TickAIRegistry();

        foreach (var entry in activeAIs)
        {
            if (entry == null || entry.brain == null)
                continue;

            if (entry.isDead)
                continue;

            var health = entry.brain.Context?.Health;
            if (health == null || health.IsDead)
                continue;

            data.AIs.Add(new AISaveData
            {
                aiId = entry.aiId,
                position = entry.brain.transform.position,
                rotation = entry.brain.transform.rotation,
                currentHp = health.CurrentHp
            });
        }

        foreach (var pair in respawnTimers)
        {
            data.timers.Add(new AIRespawnTimerSave
            {
                aiId = pair.Key,
                timeLeft = pair.Value
            });
        }

        return data;
    }

    public void RestoreState(object state)
    {
        var saved = state as SaveAIData;

        ClearAllAIs();
        BuildConfigMap();

        if (saved == null)
            return;

        // Restore saved respawn timers
        if (saved.timers != null)
        {
            foreach (var timer in saved.timers)
            {
                if (string.IsNullOrWhiteSpace(timer.aiId))
                    continue;

                respawnTimers[timer.aiId] = timer.timeLeft;
            }
        }

        // Restore alive AI instances
        if (saved.AIs == null)
            return;

        foreach (var s in saved.AIs)
        {
            if (string.IsNullOrWhiteSpace(s.aiId))
                continue;

            if (!spawnConfigById.TryGetValue(s.aiId, out var config))
            {
                GameLog.Warning(TAG, $"Restore skipped: no config for '{s.aiId}'");
                continue;
            }

            if (config.prefab == null)
            {
                GameLog.Warning(TAG, $"Restore skipped: config '{s.aiId}' has no prefab");
                continue;
            }

            GameObject go = Instantiate(config.prefab, s.position, s.rotation);
            if (go == null)
                continue;

            AgentBrain brain = go.GetComponent<AgentBrain>();
            if (brain == null)
            {
                Debug.LogError($"AIManager: prefab '{config.prefab.name}' has no AgentBrain component", go);
                Destroy(go);
                continue;
            }

            RegisterAI(s.aiId, brain);

            if (brain.Context == null || brain.Context.Health == null || brain.Config == null)
            {
                Debug.LogError($"AIManager: restored AI '{s.aiId}' has invalid Context / Health / Config", go);
                continue;
            }

            brain.Context.Health.Initialize(brain.Config.maxHp);

            int missing = Mathf.Max(0, brain.Config.maxHp - s.currentHp);
            if (missing > 0)
                brain.Context.Health.ApplyDamage(missing);
        }
    }

    #endregion
}

/// <summary>
/// Serialized save model for AI manager.
/// </summary>
[Serializable]
public class SaveAIData
{
    public List<AISaveData> AIs = new();
    public List<AIRespawnTimerSave> timers = new();
}

[Serializable]
public struct AISaveData
{
    public string aiId;
    public Vector3 position;
    public Quaternion rotation;
    public int currentHp;
}

[Serializable]
public struct AIRespawnTimerSave
{
    public string aiId;
    public float timeLeft;
}