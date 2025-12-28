using System;
using System.IO;
using System.Linq;
using UnityEngine;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }
    public string CurrentSlotId { get; private set; }

    private const string SavesFolderName = "saves";
    private const string IndexFileName = "index.json";

    private SaveIndex _index;

    private SaveGameData _pendingLoad;

    private string SavesFolderPath => Path.Combine(Application.persistentDataPath, SavesFolderName);
    private string IndexPath => Path.Combine(SavesFolderPath, IndexFileName);
    private void OnEnable()
    {
        StartCoroutine(BindOrchestratorWhenReady());
    }

    private System.Collections.IEnumerator BindOrchestratorWhenReady()
    {
        while (GameplayOrchestrator.Instance == null)
            yield return null;

        GameplayOrchestrator.Instance.OnGameplayReady -= OnGameplayReady;
        GameplayOrchestrator.Instance.OnGameplayReady += OnGameplayReady;

        Debug.Log("[Save] Bound to GameplayOrchestrator.OnGameplayReady");
    }


    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        Directory.CreateDirectory(SavesFolderPath);
        LoadIndex();

        if (GameplayOrchestrator.Instance != null)
            GameplayOrchestrator.Instance.OnGameplayReady += OnGameplayReady;
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            if (GameplayOrchestrator.Instance != null)
                GameplayOrchestrator.Instance.OnGameplayReady -= OnGameplayReady;
        }
    }


    public SaveSlotMeta[] ListSlots()
    {
        LoadIndex();
        return _index.slots
            .OrderByDescending(s => s.updatedUtcTicks)
            .ToArray();
    }

    public string CreateSlot(string displayName, string sceneName, string spawnId)
    {
        LoadIndex();

        var id = Guid.NewGuid().ToString("N");
        var now = DateTime.UtcNow.Ticks;

        var meta = new SaveSlotMeta
        {
            id = id,
            displayName = string.IsNullOrWhiteSpace(displayName) ? "New Save" : displayName.Trim(),
            createdUtcTicks = now,
            updatedUtcTicks = now,
            sceneName = sceneName,
            spawnId = spawnId,
            dataVersion = 1
        };

        _index.slots.Add(meta);
        SaveIndexToDisk();

        return id;
    }

    public string CreateBlankSlot(string displayName, string sceneName, string spawnId)
    {
        LoadIndex();

        var id = Guid.NewGuid().ToString("N");
        var now = DateTime.UtcNow.Ticks;

        var meta = new SaveSlotMeta
        {
            id = id,
            displayName = string.IsNullOrWhiteSpace(displayName) ? "New Save" : displayName.Trim(),
            createdUtcTicks = now,
            updatedUtcTicks = now,
            sceneName = sceneName,
            spawnId = spawnId,
            dataVersion = 1
        };

        _index.slots.Add(meta);
        SaveIndexToDisk();

        var data = new SaveGameData
        {
            version = 1,
            slotId = id,
            sceneName = sceneName,
            spawnId = spawnId,
            hasPlayerStats = false,
            hasPlayerTransform = false
        };

        File.WriteAllText(GetSlotPath(id), JsonUtility.ToJson(data, true));
        return id;
    }


    public bool DeleteSlot(string slotId)
    {
        LoadIndex();

        var meta = _index.slots.FirstOrDefault(s => s.id == slotId);
        if (meta == null) return false;

        _index.slots.Remove(meta);
        SaveIndexToDisk();

        var path = GetSlotPath(slotId);
        if (File.Exists(path)) File.Delete(path);

        return true;
    }

    public bool RenameSlot(string slotId, string newName)
    {
        LoadIndex();

        var meta = _index.slots.FirstOrDefault(s => s.id == slotId);
        if (meta == null) return false;

        meta.displayName = string.IsNullOrWhiteSpace(newName) ? meta.displayName : newName.Trim();
        meta.updatedUtcTicks = DateTime.UtcNow.Ticks;
        SaveIndexToDisk();
        return true;
    }

    public bool SaveToSlot(string slotId)
    {
        LoadIndex();

        var meta = _index.slots.FirstOrDefault(s => s.id == slotId);
        if (meta == null) return false;

        var orch = GameplayOrchestrator.Instance;
        if (orch == null) return false;

        var player = PlayerSpawner.Instance != null ? PlayerSpawner.Instance.Player : null;
        if (player == null) return false;

        var stats = PlayerStatManager.Instance;
        if (stats == null) return false;

        var inventoryManager = InventoryManager.Instance;
        if (inventoryManager == null) return false;

        var statusEffectManager = StatusEffectManager.Instance;
        if (statusEffectManager == null) return false;

        var worldObjectSpawner = WorldObjectSpawner.Instance;
        if (worldObjectSpawner == null) return false;

        var data = new SaveGameData
        {
            version = 1,
            slotId = slotId,
            sceneName = meta.sceneName,
            spawnId = meta.spawnId,

            hasPlayerTransform = true,
            playerTransform = new PlayerTransformSave
            {
                position = player.transform.position,
                rotation = player.transform.rotation
            },


            hasPlayerStats = true,
            playerStats = stats.Capture(),

            inventoryData = inventoryManager.Capture(),

            effectsData = statusEffectManager.CaptureAll(),

            worldItemData = worldObjectSpawner.CaptureAllWorldItems(),

            containersData = WorldContainerManager.CaptureAll()

    };

        var vcam = UnityEngine.Object.FindFirstObjectByType<Unity.Cinemachine.CinemachineCamera>();
        if (vcam != null)
        {
            var panTilt = vcam.GetComponent<Unity.Cinemachine.CinemachinePanTilt>();
            if (panTilt != null)
            {
                data.hasCameraState = true;
                data.cameraState = new CameraStateSave
                {
                    pan = panTilt.PanAxis.Value,
                    tilt = panTilt.TiltAxis.Value
                };
            }
        }


        var json = JsonUtility.ToJson(data, true);
        File.WriteAllText(GetSlotPath(slotId), json);

        meta.updatedUtcTicks = DateTime.UtcNow.Ticks;
        SaveIndexToDisk();

        return true;
    }

    public bool LoadSlot(string slotId)
    {
        var path = GetSlotPath(slotId);
        if (!File.Exists(path)) return false;

        var json = File.ReadAllText(path);
        var data = JsonUtility.FromJson<SaveGameData>(json);
        if (data == null) return false;

        _pendingLoad = data;
        Debug.Log($"[Save] LoadSlot set pending. hasT={data.hasPlayerTransform} pos={data.playerTransform.position}");

        CurrentSlotId = slotId;

        LoadIndex();
        var meta = _index.slots.FirstOrDefault(s => s.id == slotId);
        if (meta != null)
        {
            meta.updatedUtcTicks = DateTime.UtcNow.Ticks;
            SaveIndexToDisk();
        }

        var orch = GameplayOrchestrator.Instance;
        if (orch == null) return false;

        orch.LoadLocation(data.sceneName, data.spawnId);
        return true;
    }


    private void OnGameplayReady()
    {
        Debug.Log($"[Save] OnGameplayReady CALLED. pending null? {(_pendingLoad == null)}");

        if (_pendingLoad == null) return;

        var player = PlayerSpawner.Instance != null ? PlayerSpawner.Instance.Player : null;


        if (player != null && _pendingLoad.hasPlayerTransform)
        {
            var cc = player.GetComponent<CharacterController>();
            if (cc) cc.enabled = false;

            player.transform.position = _pendingLoad.playerTransform.position;

            if (_pendingLoad.hasCameraState)
            {
                var vcam = UnityEngine.Object.FindFirstObjectByType<Unity.Cinemachine.CinemachineCamera>();
                if (vcam != null)
                {
                    var panTilt = vcam.GetComponent<Unity.Cinemachine.CinemachinePanTilt>();
                    if (panTilt != null)
                    {
                        panTilt.PanAxis.Value = _pendingLoad.cameraState.pan;
                        panTilt.TiltAxis.Value = _pendingLoad.cameraState.tilt;
                    }
                }
            }


            if (cc) cc.enabled = true;
        }

        

        var inventoryManager = InventoryManager.Instance;
        if (inventoryManager != null)
        {
            inventoryManager.Restore(_pendingLoad.inventoryData);
        }

        var statusEffectManager = StatusEffectManager.Instance;
        if (statusEffectManager != null)
        {
            statusEffectManager.RestoreAll(_pendingLoad.effectsData);
        }

        var worldObjectSpawner = WorldObjectSpawner.Instance;
        if (worldObjectSpawner != null)
        {
            worldObjectSpawner.RestoreAllWorldItems(_pendingLoad.worldItemData);
        }

        if (_pendingLoad.hasPlayerStats)
        {
            var stats = PlayerStatManager.Instance;
            if (stats != null)
            {
                stats.Restore(_pendingLoad.playerStats);
                stats.RecalculateWeight();
            }
        }

        WorldContainerManager.RestoreAll(_pendingLoad.containersData);

        _pendingLoad = null;
    }


    private void LoadIndex()
    {
        if (_index != null) return;

        if (!File.Exists(IndexPath))
        {
            _index = new SaveIndex();
            SaveIndexToDisk();
            return;
        }

        var json = File.ReadAllText(IndexPath);
        _index = JsonUtility.FromJson<SaveIndex>(json) ?? new SaveIndex();
    }

    private void SaveIndexToDisk()
    {
        var json = JsonUtility.ToJson(_index, true);
        File.WriteAllText(IndexPath, json);
    }

    private string GetSlotPath(string slotId) => Path.Combine(SavesFolderPath, $"{slotId}.json");


}
