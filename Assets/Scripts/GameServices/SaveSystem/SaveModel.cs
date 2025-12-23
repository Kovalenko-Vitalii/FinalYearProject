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
}


[Serializable]
public struct PlayerTransformSave
{
    public Vector3 position;
    public Quaternion rotation;
}

[Serializable]
public struct CameraStateSave
{
    public float pan;
    public float tilt;
}

[Serializable]
public struct PlayerStatsSave
{
    public float health, hunger, hydration, energy, temperature;
}
