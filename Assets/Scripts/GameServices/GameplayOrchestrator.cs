using UnityEngine;
using System;
using System.Collections;

public class GameplayOrchestrator : MonoBehaviour
{
    public static GameplayOrchestrator Instance { get; private set; }

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
    public enum GameState { Boot, MainMenu, Loading, Gameplay }
    public GameState State { get; private set; } = GameState.Boot;


    // When entering the game - entering menu state
    private void Start()
    {
        EnterMenu();
    }

    private void Awake()
    {
        if (Instance != null && Instance != this) { 
            Destroy(gameObject); 
            return; 
        }

        Instance = this;
    }

    // Method used to enter menu state
    public void EnterMenu()
    {
        // Hiding loading
        loading?.Hide();

        // Calling action OnEnterMenu and setting game state
        OnEnterMenu?.Invoke();
        State = GameState.MainMenu;

        // Switching panels using uiState controller and loading menu scene
        uiState.EnterMainMenu();
        SceneLoader.Instance.LoadContent(menuScene);

        // Despawning player from the map and turning off ticking
        PlayerSpawner.Instance.Despawn();
        PlayerTickSystem.Instance.SetEnabled(false);
    }

    // Method used to load initial location when fresh save created
    public void StartGame()
    {
        LoadLocation(firstLevel, defaultSpawnId);
    }

    // Method used for loading location with defined player spawnpoint
    public void LoadLocation(string sceneName, string spawnId)
    {
        _nextSpawnId = string.IsNullOrEmpty(spawnId) ? defaultSpawnId : spawnId;
        StartCoroutine(LoadLocationRoutine(sceneName));
    }

    // Main method used for loading any location
    private IEnumerator LoadLocationRoutine(string sceneName)
    {
        // Set game state to loading
        State = GameState.Loading;

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

        var op = SceneLoader.Instance.LoadContentAsync(sceneName, allowSceneActivation: false);

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

        while (!Input.anyKeyDown)
            yield return null;

        loading?.ShowPressAnyKey(false);

        // =-=-=-=-=-=-=-=-=-=-=-=

        // ??? Wait till the scene loader prepare scene ???
        SceneLoader.Instance.ActivateLoadedScene();
        while (SceneLoader.Instance.IsLoading)
            yield return null;

        // Finding spawn and spawn player
        var sp = FindSpawn(_nextSpawnId) ?? FindSpawn(defaultSpawnId);
        if (sp)
        {
            PlayerSpawner.Instance.SpawnOrMoveTo(sp.transform);
            OnPlayerSpawned?.Invoke(PlayerSpawner.Instance.Player);
        }

        // Find cimenachineBinder and bind camera to headposition
        var binder = FindFirstObjectByType<CinemachineBinder>();
        binder?.BindForActivePlayer();

        // Show GUI, set game state and trigger action
        uiState.EnterGameplay();
        State = GameState.Gameplay;
        OnGameplayReady?.Invoke();

        // Hide additionaly loading and resume ticking
        loading?.Hide();
        PlayerTickSystem.Instance.SetEnabled(true);
    }

    // --- This must be moved to playerspawner i guess
    // Finding spawnpoint
    private PlayerSpawnPoint FindSpawn(string id)
    {
        var points = GameObject.FindObjectsByType<PlayerSpawnPoint>(FindObjectsSortMode.None);
        foreach (var p in points)
            if (p.Id == id) return p;
        return null;
    }
}
