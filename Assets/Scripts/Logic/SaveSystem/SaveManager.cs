using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

// This class is responsible for operating save / load system
public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }

    private const string TAG = "SaveManager";
    public string CurrentSlotId { get; private set; }

    // Path and name for save
    private const string SavesFolderName = "saves";
    private const string IndexFileName = "index.json";

    // List to file with saves
    private SaveIndex index;

    // Buffer with actual game data save
    private SaveGameData pendingLoad;

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

        GameLog.Log(TAG, "Bound to GameplayOrchestrator.OnGameplayReady");
    }


    private void Awake()
    {
        GameLog.Log(TAG, $"Awake() initiated");

        if (Instance != null && Instance != this) {
            GameLog.Warning(TAG, $"Duplicate -> destroy id={GetInstanceID()}");
            Destroy(gameObject); 
            return; 
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        Directory.CreateDirectory(SavesFolderPath);
        LoadIndex();

        if (GameplayOrchestrator.Instance != null)
        {
            GameplayOrchestrator.Instance.OnGameplayReady -= OnGameplayReady;
            GameplayOrchestrator.Instance.OnGameplayReady += OnGameplayReady;
            GameLog.Log(TAG, "Orchestrator already exists -> subscribed OnGameplayReady");
        }

        GameLog.Log(TAG, $"Awake() finished. Singleton set");
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
        if (index != null) return;

        if (!File.Exists(IndexPath))
        {
            index = new SaveIndex();
            SaveIndexToDisk();
            GameLog.Log(TAG, "Index not found -> created new index.json");
            return;
        }

        var json = File.ReadAllText(IndexPath);
        index = JsonUtility.FromJson<SaveIndex>(json) ?? new SaveIndex();

        GameLog.Log(TAG, $"Index loaded. slots={index.slots.Count}");
    }

    // Saving list of slots
    private void SaveIndexToDisk()
    {
        var json = JsonUtility.ToJson(index, true);
        File.WriteAllText(IndexPath, json);
    }

    // Return list of saved slots
    public SaveSlotMeta[] ListSlots()
    {
        LoadIndex();
        return index.slots
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

        index.slots.Add(meta);
        SaveIndexToDisk();

        GameLog.Log(TAG, $"Created slot id='{id}' name='{meta.displayName}' scene='{sceneName}' spawn='{spawnId}'");
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

        index.slots.Add(meta);
        SaveIndexToDisk();

        var data = new SaveGameData
        {
            version = 1,
            slotId = id,
            sceneName = sceneName,
            spawnId = spawnId,
            isSnapshot = false,

            playerTransform = null,
            cameraState = null,

            worldState = new SaveWorldState(),
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
        var meta = index.slots.FirstOrDefault(s => s.id == slotId);
        if (meta == null)
        {
            GameLog.Warning(TAG, $"DeleteSlot: not found slotId='{slotId}'"); 
            return false; 
        }

        // Removing slot from list (index)
        index.slots.Remove(meta);
        SaveIndexToDisk();

        // Removing slot file from folder
        var path = GetSlotPath(slotId);
        if (File.Exists(path)) File.Delete(path);

        GameLog.Log(TAG, $"Deleted slot slotId='{slotId}'");
        return true;
    }

    // Changing Save slot name (same as deleting pretty much)
    public bool RenameSlot(string slotId, string newName)
    {
        LoadIndex();

        var meta = index.slots.FirstOrDefault(s => s.id == slotId);
        if (meta == null)
        {
            GameLog.Warning(TAG, $"RenameSlot: not found slotId='{slotId}'");
            return false;
        }

        var old = meta.displayName;
        meta.displayName = string.IsNullOrWhiteSpace(newName) ? meta.displayName : newName.Trim();
        meta.updatedUtcTicks = DateTime.UtcNow.Ticks;
        SaveIndexToDisk();

        GameLog.Log(TAG, $"RenameSlot slotId='{slotId}' '{old}' -> '{meta.displayName}'");
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
        var meta = index.slots.FirstOrDefault(s => s.id == slotId);
        if (meta == null)
        {
            GameLog.Error(TAG, $"SaveToSlot failed: meta not found slotId='{slotId}'");
            return false;
        }

        // Ensuring we have all necessary instances to gather save data from them
        var orch = GameplayOrchestrator.Instance;
        if (orch == null) { GameLog.Error(TAG, "SaveToSlot failed: GameplayOrchestrator.Instance is NULL"); return false; }

        var spawner = PlayerSpawner.Instance;
        if (spawner == null) { GameLog.Error(TAG, "SaveToSlot failed: PlayerSpawner.Instance is NULL"); return false; }

        var player = spawner.Player;
        if (player == null) { GameLog.Error(TAG, "SaveToSlot failed: spawner.Player is NULL"); return false; }    

        GameLog.Log(TAG, $"SaveToSlot BEGIN slot='{slotId}' scene='{currentScene}'");


        // Creating new GameData and saving information to it
        var data = new SaveGameData
        {
            version = 1,
            slotId = slotId,
            sceneName = currentScene,
            spawnId = "level01",
            isSnapshot = true,

            playerTransform = new PlayerTransformSave
            {
                position = player.transform.position,
                rotation = player.transform.rotation
            },

            worldState = SaveRegistry.CaptureAll(),
        };

        // Setting up camera rotation if camera has this parameter on scene (I know it is poorly made :-)
        var vcam = GameObject.FindFirstObjectByType<Unity.Cinemachine.CinemachineCamera>();
        if (vcam != null)
        {
            var panTilt = vcam.GetComponent<Unity.Cinemachine.CinemachinePanTilt>();
            if (panTilt != null)
            {
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

        GameLog.Log(TAG, $"SaveToSlot END slot='{slotId}' file='{GetSlotPath(slotId)}'");
        return true;
    }

    // Loading data from slot to buffer and spawning to location 
    public bool LoadSlot(string slotId)
    {
        // Getting path and checking if the file exist
        var path = GetSlotPath(slotId);
        if (!File.Exists(path))
        {
            GameLog.Error(TAG, $"LoadSlot failed: file not found '{path}'");
            return false;
        }

        // Gathering data
        var json = File.ReadAllText(path);
        var data = JsonUtility.FromJson<SaveGameData>(json);
        if (data == null)
        {
            GameLog.Error(TAG, $"LoadSlot failed: JSON parse returned NULL slotId='{slotId}'");
            return false;
        }

        // Putting data into buffer and index
        pendingLoad = data;
        CurrentSlotId = slotId;
        GameLog.Log(TAG, $"LoadSlot set pending slot='{slotId}' scene='{data.sceneName}'");

        // Loading list of saves and updating time of slot has been changed
        LoadIndex();
        var meta = index.slots.FirstOrDefault(s => s.id == slotId);
        if (meta != null)
        {
            meta.updatedUtcTicks = DateTime.UtcNow.Ticks;
            SaveIndexToDisk();
        }

        var orch = GameplayOrchestrator.Instance;
        if (orch == null)
        {
            GameLog.Error(TAG, "LoadSlot failed: GameplayOrchestrator.Instance is NULL");
            return false;
        }

        // I`m not sure that this is really bad, but maybe this script should not trigger location loading
        if (data.isSnapshot)
        {
            orch.MarkNextLoadAsSave();
            orch.LoadLocationFromSave(data.sceneName);
        }
        else
        {
            orch.LoadLocation(data.sceneName, data.spawnId);
        }

        GameLog.Log(TAG, $"LoadSlot set pending slot='{slotId}' scene='{data.sceneName}' hasT={(data.playerTransform != null)}");
        return true;
    }

    // Basically loading data from buffered save to game
    private void OnGameplayReady()
    {
        // Checking if there is stored data in buffer
        if (pendingLoad == null) return;

        GameLog.Log(TAG, $"ApplyPendingLoad BEGIN scene='{pendingLoad.sceneName}'");

        // Setting up cinemachine rotation
        // --- I think teleporting player should be here
        if (pendingLoad.cameraState != null)
        {
            var vcam = GameObject.FindFirstObjectByType<Unity.Cinemachine.CinemachineCamera>();
            if (vcam != null)
            {
                var panTilt = vcam.GetComponent<Unity.Cinemachine.CinemachinePanTilt>();
                if (panTilt != null)
                {
                    panTilt.PanAxis.Value = pendingLoad.cameraState.pan;
                    panTilt.TiltAxis.Value = pendingLoad.cameraState.tilt;
                }
            }
        }

        // Restoring all necessary data
        if (pendingLoad.isSnapshot)
        {
            var playerSpawner = PlayerSpawner.Instance;
            if (playerSpawner != null)
            {
                playerSpawner.SpawnOrMoveTo(
                    pendingLoad.playerTransform.position,
                    pendingLoad.playerTransform.rotation
                );
            }
        }

        SaveRegistry.RestoreAll(pendingLoad.worldState);

        pendingLoad = null;
        GameLog.Log(TAG, "ApplyPendingLoad END (pending cleared)");
    }

    public void LoadLastSlot()
    {
        LoadIndex();

        var last = index.slots
            .OrderByDescending(s => s.updatedUtcTicks)
            .FirstOrDefault();

        if (last == null)
        {
            GameLog.Warning(TAG, "LoadLastSlot: no slots");
            return;
        }

        LoadSlot(last.id);
    }
}