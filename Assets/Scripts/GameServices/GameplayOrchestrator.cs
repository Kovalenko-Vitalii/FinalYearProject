using UnityEngine;
using System;
using System.Collections;

public class GameplayOrchestrator : MonoBehaviour
{
    public static GameplayOrchestrator Instance { get; private set; }

    public event Action OnEnterMenu;
    public event Action<string> OnLoadingStarted;
    public event Action<GameObject> OnPlayerSpawned;
    public event Action OnGameplayReady;

    [SerializeField] private UIStateController uiState;
    [SerializeField] private string menuScene = "MainMenu";
    [SerializeField] private string firstLevel = "Level01";
    [SerializeField] private string defaultSpawnId = "Start";

    [SerializeField] private string _nextSpawnId = "level01";

    public enum GameState { Boot, MainMenu, Loading, Gameplay }
    public GameState State { get; private set; } = GameState.Boot;

    private void Start()
    {
        EnterMenu();
    }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void EnterMenu()
    {
        OnEnterMenu?.Invoke();
        State = GameState.MainMenu;
        uiState.EnterMainMenu();
        SceneLoader.Instance.LoadContent(menuScene);
        PlayerSpawner.Instance.Despawn();
        PlayerTickSystem.Instance.SetEnabled(false);
    }

    public void StartGame()
    {
        LoadLocation(firstLevel, defaultSpawnId);
    }

    public void LoadLocation(string sceneName, string spawnId)
    {
        _nextSpawnId = string.IsNullOrEmpty(spawnId) ? defaultSpawnId : spawnId;
        StartCoroutine(LoadLocationRoutine(sceneName));
    }

    private IEnumerator LoadLocationRoutine(string sceneName)
    {
        OnLoadingStarted?.Invoke(sceneName);

        State = GameState.Loading;

        uiState.EnterGameplay();

        yield return SceneLoader.Instance.LoadContent(sceneName);

        Debug.Log($"[Orch] Loaded scene {sceneName}, nextSpawnId={_nextSpawnId}");
        var sp = FindSpawn(_nextSpawnId) ?? FindSpawn(defaultSpawnId);
        Debug.Log($"[Orch] SpawnPoint found? {(sp ? sp.name : "NULL")} pos={(sp ? sp.transform.position.ToString() : "n/a")}");

        if (sp) { PlayerSpawner.Instance.SpawnOrMoveTo(sp.transform);
            var player = PlayerSpawner.Instance.Player;
            OnPlayerSpawned?.Invoke(player);
        }

        Debug.Log($"[Orch] After SpawnOrMoveTo, player pos={PlayerSpawner.Instance.Player.transform.position}");

        var binder = UnityEngine.Object.FindFirstObjectByType<CinemachineBinder>();
        Debug.Log($"[Orch] CinemachineBinder found? {(binder ? binder.name : "NULL")}");
        binder?.BindForActivePlayer();
        yield return null;

        State = GameState.Gameplay;
        Debug.Log("[Orch] OnGameplayReady invoke");
        OnGameplayReady?.Invoke();

        PlayerTickSystem.Instance.SetEnabled(true);
    }

    private PlayerSpawnPoint FindSpawn(string id)
    {
        var points = UnityEngine.Object.FindObjectsByType<PlayerSpawnPoint>(FindObjectsSortMode.None);
        foreach (var p in points)
            if (p.Id == id) return p;
        return null;
    }
}
