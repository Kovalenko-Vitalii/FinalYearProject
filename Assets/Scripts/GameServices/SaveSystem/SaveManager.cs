using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }
    public string CurrentSlotId { get; private set; }


    // Path and name for save
    private const string SavesFolderName = "saves";
    private const string IndexFileName = "index.json";

    // List to file with saves
    private SaveIndex _index;

    // Buffer with actual game data save
    private SaveGameData _pendingLoad;

    private string SavesFolderPath => Path.Combine(Application.persistentDataPath, SavesFolderName);
    private string IndexPath => Path.Combine(SavesFolderPath, IndexFileName);

    private string GetSlotPath(string slotId) => Path.Combine(SavesFolderPath, $"{slotId}.json");
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
        if (Instance != null && Instance != this) { 
            Destroy(gameObject); 
            return; 
        }
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

    // Loading list of slots
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

    // Saving list of slots
    private void SaveIndexToDisk()
    {
        var json = JsonUtility.ToJson(_index, true);
        File.WriteAllText(IndexPath, json);
    }

    // Return list of saved slots
    public SaveSlotMeta[] ListSlots()
    {
        LoadIndex();
        return _index.slots
            .OrderByDescending(s => s.updatedUtcTicks)
            .ToArray();
    }

    // Creating save slot
    public string CreateSlot(string displayName, string sceneName, string spawnId)
    {
        // Loading index from folder to buffer
        LoadIndex();

        // Rettreiving new id and time
        var id = Guid.NewGuid().ToString("N");
        var now = DateTime.UtcNow.Ticks;

        // Create a new metadata
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
            hasPlayerTransform = false,
            dateWeatherSave = new DateWeatherSave { day = 1, minutes = 1000f }, 
        };

        File.WriteAllText(GetSlotPath(id), JsonUtility.ToJson(data, true));
        return id;
    }

    // Deleting Save Slot
    public bool DeleteSlot(string slotId)
    {
        // Loading list of slots to buffer
        LoadIndex();

        // Finding slot by id
        var meta = _index.slots.FirstOrDefault(s => s.id == slotId);
        if (meta == null) return false;

        // Removing slot from list (index)
        _index.slots.Remove(meta);
        SaveIndexToDisk();

        // Removing slot file from folder
        var path = GetSlotPath(slotId);
        if (File.Exists(path)) File.Delete(path);

        return true;
    }

    // Changing Save slot name (same as deleting pretty much)
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

    // Saving game information to slot
    public bool SaveToSlot(string slotId)
    {
        // Loading list of slots to buffer
        LoadIndex();

        var currentScene = SceneLoader.Instance != null
        ? SceneLoader.Instance.CurrentContentScene
        : SceneManager.GetActiveScene().name;

        if (string.IsNullOrEmpty(currentScene))
            currentScene = SceneManager.GetActiveScene().name;

        // Finding slot
        var meta = _index.slots.FirstOrDefault(s => s.id == slotId);
        if (meta == null) return false;

        // Ensuring we have all necessary instances to gather save data from them
        var orch = GameplayOrchestrator.Instance;
        if (orch == null) return false;

        var spawner = PlayerSpawner.Instance;
        if (spawner == null) return false;

        var player = spawner.Player;
        if (player == null) return false;

        var stats = PlayerStatManager.Instance;
        if (stats == null) return false;

        var inventoryManager = InventoryManager.Instance;
        if (inventoryManager == null) return false;

        var statusEffectManager = StatusEffectManager.Instance;
        if (statusEffectManager == null) return false;

        var worldObjectSpawner = WorldObjectSpawner.Instance;
        if (worldObjectSpawner == null) return false;

        var dateWeatherManager = DateWeatherManager.Instance;
        if (dateWeatherManager == null) return false;

        // Creating new GameData and saving information to it
        var data = new SaveGameData
        {
            version = 1,
            slotId = slotId,
            sceneName = currentScene,
            spawnId = null,

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

            containersData = WorldContainerManager.CaptureAll(),

            doorsData = DoorSaveSystem.CaptureAll(),

            dateWeatherSave = dateWeatherManager.Capture(),

        };

        // Setting up camera rotation if camera has this parameter on scene (I know it is poorly made :-)
        var vcam = GameObject.FindFirstObjectByType<Unity.Cinemachine.CinemachineCamera>();
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

    // Loading data from slot to buffer and spawning to location 
    public bool LoadSlot(string slotId)
    {
        // Getting path and checking if the file exist
        var path = GetSlotPath(slotId);
        if (!File.Exists(path)) return false;

        // Gathering data
        var json = File.ReadAllText(path);
        var data = JsonUtility.FromJson<SaveGameData>(json);
        if (data == null) return false;

        // Putting data into buffer and index
        _pendingLoad = data;
        CurrentSlotId = slotId;
        Debug.Log($"[Save] LoadSlot set pending. hasT={data.hasPlayerTransform} pos={data.playerTransform.position}");
        
        // Loading list of saves and updating time of slot has been changed
        LoadIndex();
        var meta = _index.slots.FirstOrDefault(s => s.id == slotId);
        if (meta != null)
        {
            meta.updatedUtcTicks = DateTime.UtcNow.Ticks;
            SaveIndexToDisk();
        }

        var orch = GameplayOrchestrator.Instance;
        if (orch == null) return false;

        // I`m not sure that this is really bad, but maybe this script should not trigger location loading
        orch.MarkNextLoadAsSave();
        orch.LoadLocation(data.sceneName, null);
        return true;
    }

    // Basically loading data from buffered save to game
    private void OnGameplayReady()
    {
        Debug.Log($"[Save] OnGameplayReady CALLED. pending null? {(_pendingLoad == null)}");
        // Checking if there is stored data in buffer
        if (_pendingLoad == null) return;
      
        
        // Setting up cinemachine rotation
        // --- I think teleporting player should be here
        if (_pendingLoad.hasCameraState)
        {
            var vcam = GameObject.FindFirstObjectByType<Unity.Cinemachine.CinemachineCamera>();
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

        // Restoring all necessary data
        var playerSpawner = PlayerSpawner.Instance;
        if (playerSpawner != null && _pendingLoad.hasPlayerTransform)
            playerSpawner.SpawnOrMoveTo(
                _pendingLoad.playerTransform.position,
                _pendingLoad.playerTransform.rotation
            );

        var active = SceneManager.GetActiveScene();
        CinemachineBinder binder = null;
        foreach (var root in active.GetRootGameObjects())
        {
            binder = root.GetComponentInChildren<CinemachineBinder>(true);
            if (binder) break;
        }
        binder?.BindForActivePlayer();


        var inventoryManager = InventoryManager.Instance;
        if (inventoryManager != null && _pendingLoad.inventoryData.inventoryItems.Count > 0)
            inventoryManager.Restore(_pendingLoad.inventoryData);

        var statusEffectManager = StatusEffectManager.Instance;
        if (statusEffectManager != null)
            statusEffectManager.RestoreAll(_pendingLoad.effectsData);

        var worldObjectSpawner = WorldObjectSpawner.Instance;
        if (worldObjectSpawner != null)
            worldObjectSpawner.RestoreAllWorldItems(_pendingLoad.worldItemData);

        var dateWeatherManager = DateWeatherManager.Instance;
        if (dateWeatherManager != null)
            dateWeatherManager.Restore(_pendingLoad.dateWeatherSave);


        // --- Flags like hasPlayerStats should be removed i think
        if (_pendingLoad.hasPlayerStats)
        {
            var stats = PlayerStatManager.Instance;
            if (stats != null)
            {
                stats.Restore(_pendingLoad.playerStats);
            }
        }

        WorldContainerManager.RestoreAll(_pendingLoad.containersData);
        DoorSaveSystem.RestoreAll(_pendingLoad.doorsData);

        _pendingLoad = null;
    }
}
