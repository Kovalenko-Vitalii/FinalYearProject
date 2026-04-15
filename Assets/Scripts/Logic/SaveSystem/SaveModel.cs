using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

// List of save slots, used for quick retreiving list of slot`s meta data, just for visualisation
[Serializable]
public class SaveIndex
{
    public int version = 1;
    public List<SaveSlotMeta> slots = new();
}

// Save slot meta data, linked to game data using same ID
[Serializable]
public class SaveSlotMeta
{
    // Foreign key
    public string id;
    public string displayName;
    // Time mark for when created and last update
    public long createdUtcTicks;
    public long updatedUtcTicks;

    // Name of scene and spawn Id
    public string sceneName;
    public string spawnId;

    // Version for migration
    public int dataVersion = 1;
}

// Game data
[Serializable]
public class SaveGameData
{
    public int version = 1;

    // Foreign key
    public string slotId;
    public string sceneName;

    public bool isSnapshot;
    public string spawnId;


    // --- I think that flags should be removed
    // Other data parts
    public PlayerTransformSave playerTransform;
    public CameraStateSave cameraState;

    public SaveWorldState worldState;
}

// Saving player position
[Serializable]
public class PlayerTransformSave
{
    public Vector3 position;
    public Quaternion rotation;
}

// Saving camera rotation
[Serializable]
public class CameraStateSave
{
    public float pan;
    public float tilt;
}

[Serializable]
public class WorldStateEntry
{
    public string id;   // SaveId
    public string type; // AssemblyQualifiedName
    public string json; // JsonUtility.ToJson(state)
}

[Serializable]
public class SaveWorldState
{
    public List<WorldStateEntry> entries = new();
}

public interface ISaveable
{
    string SaveId { get; }
    object CaptureState();
    void RestoreState(object state);
    void ResetToDefaultState();
}

public static class SaveRegistry
{
    public static SaveWorldState CaptureAll()
    {
        var result = new SaveWorldState();

        var saveables = UnityEngine.Object.FindObjectsByType<MonoBehaviour>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None
            ).OfType<ISaveable>();

        var map = new Dictionary<string, WorldStateEntry>();

        foreach (var s in saveables)
        {
            if (string.IsNullOrWhiteSpace(s.SaveId)) continue;

            var state = s.CaptureState();
            if (state == null) continue;

            var entry = new WorldStateEntry
            {
                id = s.SaveId,
                type = state.GetType().AssemblyQualifiedName,
                json = JsonUtility.ToJson(state)
            };

            if (map.ContainsKey(entry.id))
                Debug.LogWarning($"Duplicate SaveId during capture: {entry.id} (overwriting previous entry)");

            map[entry.id] = entry;
        }

        result.entries = map.Values.ToList();
        return result;
    }

    public static void RestoreAll(SaveWorldState state)
    {
        if (state == null) return;

        var saveablesList = UnityEngine.Object.FindObjectsByType<MonoBehaviour>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None
            )
            .OfType<ISaveable>()
            .Where(s => !string.IsNullOrWhiteSpace(s.SaveId))
            .ToList();

        foreach (var s in saveablesList)
            s.ResetToDefaultState();

        var dupSaveables = saveablesList
            .GroupBy(s => s.SaveId)
            .Where(g => g.Count() > 1)
            .ToList();

        foreach (var g in dupSaveables)
        {
            var names = string.Join(", ", g.Select(x => (x as MonoBehaviour)?.name ?? x.ToString()));
            Debug.LogError($"Duplicate SaveId in scene: {g.Key}. Objects: {names}");
        }

        var saveables = saveablesList
            .GroupBy(s => s.SaveId)
            .ToDictionary(g => g.Key, g => g.First());

        var entries = state.entries
            .Where(e => !string.IsNullOrWhiteSpace(e.id))
            .GroupBy(e => e.id)
            .Select(g =>
            {
                if (g.Count() > 1)
                    Debug.LogWarning($"Duplicate saved entry id in file: {g.Key}. Using last occurrence.");
                return g.Last();
        });

        foreach (var e in entries)
        {
            if (!saveables.TryGetValue(e.id, out var target)) continue;

            var t = Type.GetType(e.type);
            if (t == null) continue;

            var payload = JsonUtility.FromJson(e.json, t);
            target.RestoreState(payload);
        }
    }

    public static void ResetAllToDefaults()
    {
        var saveables = UnityEngine.Object.FindObjectsByType<MonoBehaviour>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None
            )
            .OfType<ISaveable>()
            .Where(s => !string.IsNullOrWhiteSpace(s.SaveId));

        foreach (var s in saveables)
            s.ResetToDefaultState();
    }
}