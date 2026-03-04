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

    public PlayerStatsSave playerStats;

    public CameraStateSave cameraState;

    public SaveInventoryData inventoryData;

    public SaveEffectsData effectsData;

    public SaveWorldItemsData worldItemData;

    public SaveWorldState worldState;

}

// Saving player inventory and gear
[Serializable]
public class SaveInventoryData
{
    public List<InventoryItemSave> inventoryItems = new();
    public List<GearPairSave> gearSlots = new();
}

[Serializable]
public struct InventoryItemSave
{
    public string itemId;
    public int amount;
    public float durability;
}

// --- this could be inherited from InventoryItemSave
[Serializable]
public struct GearPairSave
{
    public GearData.GearSlot slot;
    public string gearId;
    public float durability;
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

// Saving player stats
[Serializable]
public class PlayerStatsSave
{
    public float health, hunger, hydration, energy, temperature, stamina;
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

// Saving player`s effects
[Serializable]
public class SaveEffectsData
{
    public List<EffectSave> effectList = new();
}

// --- This system helps me to serialize different effects that has custom parameters, the best I could invent in this situation
[Serializable]
public class EffectSave
{
    public StatusEffectId id;
    public float duration;

    public bool hasTarget;
    public BodyPart target;

    public string payloadJson;
}

[Serializable]
public class BleedingSave
{
    public float dps;
}

[Serializable]
public class FractureSave
{
    public float speedMultiplier;
}

[Serializable]
public class PainSave
{
    public float intensity;
    public float buildup;
}

[Serializable]
public class WorldStateEntry
{
    public string id;   // SaveId
    public string type; // AssemblyQualifiedName
    public string json; // JsonUtility.ToJson(state)
}

[System.Serializable]
public class SaveWorldState
{
    public System.Collections.Generic.List<WorldStateEntry> entries = new();
}

public interface ISaveable
{
    string SaveId { get; }
    object CaptureState();
    void RestoreState(object state);
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

        foreach (var s in saveables)
        {
            if (string.IsNullOrWhiteSpace(s.SaveId)) continue;

            var state = s.CaptureState();
            if (state == null) continue;

            result.entries.Add(new WorldStateEntry
            {
                id = s.SaveId,
                type = state.GetType().AssemblyQualifiedName,
                json = JsonUtility.ToJson(state)
            });
        }

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
}