using UnityEngine;
using System;
using System.Collections;

public class GameplayOrchestrator : MonoBehaviour
{
    string TAG = "GameplayOrchestrator";
    public static GameplayOrchestrator Instance { get; private set; }
    private bool _isSaveLoad;

    // Loading screen UI
    [SerializeField] private LoadingOverlay loading;

    // Actions for indicating stages of the game
    public event Action OnEnterMenu;
    public event Action<string> OnLoadingStarted;
    public event Action<GameObject> OnPlayerSpawned;
    public event Action OnGameplayReady;

    // Menu and GUI switcher/controller
    [SerializeField] private UIStateController uiState;
    
    // Menu scene/Initial location/Initial spawn id
    [SerializeField] private string menuScene = "MainMenu";
    [SerializeField] private string firstLevel = "Level01";
    [SerializeField] private string defaultSpawnId = "Start";

    // Buffer spawn id for transition between locations
    [SerializeField] private string _nextSpawnId = "level01";

    [Header("Loading behaviour")]
    [SerializeField] private float minLoadingScreenTime = 0.6f;

    // Game state for logic
    public enum GameState { Boot, MainMenu, Loading, Gameplay, Died }
    public GameState State { get; private set; } = GameState.Boot;

    private void Awake()
    {
        GameLog.Log(TAG, $"Awake() initiated");

        if (Instance != null && Instance != this) {
            GameLog.Warning(TAG, $"Duplicate instance detected -> destroying id={GetInstanceID()}");
            Destroy(gameObject); 
            return; 
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        EnterMenu();

        GameLog.Log(TAG, $"Awake() finished. Singleton set");
    }

    // Method used to enter menu state
    public void EnterMenu()
    {
        GameLog.Log(TAG, $"EnterMenu() from state={State} -> MainMenu. Will load '{menuScene}'");

        loading?.Hide(); // Hiding loading ui

        // Calling action OnEnterMenu and setting game state
        OnEnterMenu?.Invoke();
        State = GameState.MainMenu;

        // Switching panels using uiState controller and loading menu scene
        // Despawning player from the map and turning off ticking
        if (uiState == null)
            GameLog.Warning(TAG, "uiState is NULL (cannot switch UI states)");
        else
            uiState.EnterMainMenu();

        if (SceneLoader.Instance == null)
            GameLog.Error(TAG, "SceneLoader.Instance is NULL (cannot load menu scene)");
        else
            SceneLoader.Instance.LoadContent(menuScene);

        if (PlayerSpawner.Instance == null)
            GameLog.Warning(TAG, "PlayerSpawner.Instance is NULL (cannot despawn)");
        else
            PlayerSpawner.Instance.Despawn();

        if (PlayerTickSystem.Instance == null)
            GameLog.Warning(TAG, "PlayerTickSystem.Instance is NULL (cannot disable ticking)");
        else
            PlayerTickSystem.Instance.SetEnabled(false);
    }

    // Method used to load initial location when fresh save created
    public void StartGame() 
    {
        GameLog.Log(TAG, $"StartGame -> level='{firstLevel}' spawn='{defaultSpawnId}'");

        LoadLocation(firstLevel, defaultSpawnId); 
    }

    // Method used for loading location with defined player spawnpoint
    public void LoadLocation(string sceneName, string spawnId)
    {
        if (State == GameState.Loading)
        {
            GameLog.Warning(TAG, "LoadLocation ignored: already loading");
            return;
        }

        _isSaveLoad = false;
        _nextSpawnId = spawnId;

        GameLog.Log(TAG, $"LoadLocation(scene='{sceneName}', spawn='{spawnId}') isSaveLoad={_isSaveLoad}");

        if (string.IsNullOrEmpty(sceneName))
        {
            GameLog.Error(TAG, "LoadLocation called with EMPTY sceneName");
            return;
        }

        StartCoroutine(LoadLocationRoutine(sceneName));
    }


    // Main method used for loading any location
    private IEnumerator LoadLocationRoutine(string sceneName)
    {
        GameLog.Log(TAG, $"LoadLocationRoutine BEGIN scene='{sceneName}' isSaveLoad={_isSaveLoad}");

        State = GameState.Loading; // Set game state to loading

        // Enable loading screen, set progress bar at 0 and deactivate PressAnyKey label
        loading?.Show();
        loading?.SetProgress(0f);
        loading?.ShowPressAnyKey(false);

        // Turn off ticking and trigger action
        PlayerTickSystem.Instance.SetEnabled(false);
        OnLoadingStarted?.Invoke(sceneName);

        // =-=-=-=-=-=-=-=-=-=-=-=
        // --- This whole block does not belong here I know. WIP
        float startTime = Time.unscaledTime;
        if (SceneLoader.Instance == null)
        {
            GameLog.Error(TAG, "SceneLoader.Instance is NULL inside LoadLocationRoutine");
            yield break;
        }

        var op = SceneLoader.Instance.LoadContentAsync(sceneName, allowSceneActivation: false);
        GameLog.Log(TAG, $"Scene async load started (allowActivation=false)");

        float visual = 0f;

        while (op != null && op.progress < 0.9f)
        {
            float p = Mathf.Clamp01(op.progress / 0.9f);
            visual = Mathf.MoveTowards(visual, p, Time.unscaledDeltaTime * 2f);
            loading?.SetProgress(visual);
            yield return null;
        }

        while (Time.unscaledTime - startTime < minLoadingScreenTime)
        {
            float speed = 1f / Mathf.Max(0.01f, minLoadingScreenTime);
            visual = Mathf.MoveTowards(visual, 1f, Time.unscaledDeltaTime * speed);
            loading?.SetProgress(visual);
            yield return null;
        }

        loading?.SetProgress(1f);

        loading?.ShowPressAnyKey(true);
        GameLog.Log(TAG, "Scene loaded to 0.9 -> waiting PressAnyKey");

        while (!Input.anyKeyDown)
            yield return null;

        loading?.ShowPressAnyKey(false);
        GameLog.Log(TAG, "PressAnyKey received -> activating loaded scene");
        // =-=-=-=-=-=-=-=-=-=-=-=

        // ??? Wait till the scene loader prepare scene ???
        SceneLoader.Instance.ActivateLoadedScene();
        while (SceneLoader.Instance.IsLoading)
            yield return null;

        yield return null;

        GameLog.Log(TAG, $"Scene swap finished. CurrentContentScene='{SceneLoader.Instance.CurrentContentScene}' activeScene='{UnityEngine.SceneManagement.SceneManager.GetActiveScene().name}'");

        // 1) player should exist always
        if (PlayerSpawner.Instance == null)
        {
            GameLog.Error(TAG, "PlayerSpawner.Instance is NULL (cannot EnsureSpawned)");
            yield break;
        }
        var player = PlayerSpawner.Instance.EnsureSpawned();
        GameLog.Log(TAG, $"EnsureSpawned done");

        // 2) if it is not saveLoad than we use spawnpoint registry
        if (!_isSaveLoad)
        {
            if (SpawnPointRegistry.Instance == null)
            {
                GameLog.Warning(TAG, "SpawnPointRegistry.Instance is NULL (cannot resolve spawnpoint)");
            }
            else
            {
                var spawn = SpawnPointRegistry.Instance.Get(_nextSpawnId) ?? SpawnPointRegistry.Instance.Get(defaultSpawnId);

                if (spawn != null)
                {
                    PlayerSpawner.Instance.SpawnOrMoveTo(spawn);
                    GameLog.Log(TAG, $"SpawnOrMoveTo spawnId='{spawn}' (next='{_nextSpawnId}', default='{defaultSpawnId}')");
                }
                else
                {
                    GameLog.Warning(TAG, $"Spawn NOT found (next='{_nextSpawnId}', default='{defaultSpawnId}') in scene='{sceneName}'");
                }
            }
        }
        else
        {
            GameLog.Log(TAG, "isSaveLoad=true -> spawnpoint move skipped (SaveManager will teleport player on OnGameplayReady)");
        }

        // 3) always invoke action
        OnPlayerSpawned?.Invoke(player);

        


        // Find cimenachineBinder and bind camera to headposition
        var binder = FindFirstObjectByType<CinemachineBinder>();
        if (binder == null)
            GameLog.Warning(TAG, "CinemachineBinder NOT found");
        else
        {
            binder.BindForActivePlayer();
            GameLog.Log(TAG, "CinemachineBinder binded");
        }

        // Show GUI, set game state and trigger action
        if (uiState == null)
            GameLog.Warning(TAG, "uiState is NULL (cannot EnterGameplay)");
        else
            uiState.EnterGameplay();

        State = GameState.Gameplay;

        GameLog.Log(TAG, "OnGameplayReady() invoke");
        OnGameplayReady?.Invoke();

        // 4) drop flag after spawn
        _isSaveLoad = false;

        // Hide additionaly loading and resume ticking
        loading?.Hide();
        PlayerTickSystem.Instance.SetEnabled(true);
        GameLog.Log(TAG, "LoadLocationRoutine END -> Gameplay (ticking enabled, loading hidden)");
    }

    public void MarkNextLoadAsSave()
    {
        GameLog.Log(TAG, "MarkNextLoadAsSave() -> isSaveLoad=true");

        _isSaveLoad = true;
    }

    public void EnterDied()
    {
        if (State == GameState.Died) return;

        GameLog.Log(TAG, $"EnterDied() from state={State}");

        State = GameState.Died;

        if (PlayerTickSystem.Instance != null)
            PlayerTickSystem.Instance.SetEnabled(false);

        // UI
        uiState?.EnterDied();

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void LoadLocationFromSave(string sceneName)
    {
        if (State == GameState.Loading) return;

        _nextSpawnId = null;
        StartCoroutine(LoadLocationRoutine(sceneName));
    }

    public void ReloadLastSave()
    {
        if (State == GameState.Loading) return;

        loading?.Show();
        PlayerTickSystem.Instance?.SetEnabled(false);

        SaveManager.Instance?.LoadLastSlot();
    }
}
