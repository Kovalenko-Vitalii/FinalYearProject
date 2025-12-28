using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SaveIndex
{
    public int version = 1;
    public List<SaveSlotMeta> slots = new();
}

[Serializable]
public class SaveSlotMeta
{
    public string id;
    public string displayName;
    public long createdUtcTicks;
    public long updatedUtcTicks;

    public string sceneName;
    public string spawnId;

    public int dataVersion = 1;
}

[Serializable]
public class SaveGameData
{
    public int version = 1;

    public string slotId;
    public string sceneName;
    public string spawnId;

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
    public float health, hunger, hydration, energy, temperature;
}

// Saving items on location

[Serializable]
public class SaveWorldItemsData
{
    public List<WorldItemSave> items = new();
}

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
