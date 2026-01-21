using System;
using System.Collections.Generic;
using UnityEngine;

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
    public string spawnId;

    // --- I think that flags should be removed
    // Other data parts
    public bool hasPlayerTransform;
    public PlayerTransformSave playerTransform;

    public bool hasPlayerStats;
    public PlayerStatsSave playerStats;

    public bool hasCameraState;
    public CameraStateSave cameraState;

    public SaveInventoryData inventoryData;

    public SaveEffectsData effectsData;

    public SaveWorldItemsData worldItemData;

    public SaveWorldContainersData containersData;

    public SaveDoorsData doorsData;
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
public struct PlayerTransformSave
{
    public Vector3 position;
    public Quaternion rotation;
}

// Saving camera rotation
[Serializable]
public struct CameraStateSave
{
    public float pan;
    public float tilt;
}

// Saving player stats

[Serializable]
public struct PlayerStatsSave
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
[System.Serializable]
public struct WorldItemSave
{
    public string itemId; 
    public Vector3 position;
    public Quaternion rotation;
    public int amount;
    public float durability;
}

// Saving contanier`s content

[Serializable]
public class SaveWorldContainersData
{
    public List<ContainerSave> containers = new();
}

[Serializable]
public class ContainerSave
{
    public string containerId;
    public List<InventoryItemSave> items = new();
}

// Saving player`s effects

[Serializable]
public class SaveEffectsData
{
    public List<EffectSave> effectList = new();
}

// Saving doors
[Serializable]
public class SaveDoorsData
{
    public List<DoorSave> doors = new();
}

[Serializable]
public struct DoorSave
{
    public string id;
    public bool isOpen;
    public bool isLocked;
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
