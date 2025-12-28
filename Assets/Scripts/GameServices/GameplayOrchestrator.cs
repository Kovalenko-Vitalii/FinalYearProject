using UnityEngine;
using System;
using System.Collections;

public class GameplayOrchestrator : MonoBehaviour
{
    public static GameplayOrchestrator Instance { get; private set; }

    [SerializeField] private LoadingOverlay loading;

    public event Action OnEnterMenu;
    public event Action<string> OnLoadingStarted;
    public event Action<GameObject> OnPlayerSpawned;
    public event Action OnGameplayReady;

    [SerializeField] private UIStateController uiState;
    [SerializeField] private string menuScene = "MainMenu";
    [SerializeField] private string firstLevel = "Level01";
    [SerializeField] private string defaultSpawnId = "Start";

    [SerializeField] private string _nextSpawnId = "level01";

    [Header("Loading behaviour")]
    [SerializeField] private float minLoadingScreenTime = 0.6f;
    [SerializeField] private bool requirePressAnyKey = true;

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
        loading?.Hide();

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
        State = GameState.Loading;

        loading?.Show();
        loading?.SetProgress(0f);
        loading?.ShowPressAnyKey(false);

        PlayerTickSystem.Instance.SetEnabled(false);
        OnLoadingStarted?.Invoke(sceneName);

        yield return null;

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

        if (requirePressAnyKey)
        {
            loading?.ShowPressAnyKey(true);

            while (!Input.anyKeyDown)
                yield return null;

            loading?.ShowPressAnyKey(false);
        }

        SceneLoader.Instance.ActivateLoadedScene();

        while (SceneLoader.Instance.IsLoading)
            yield return null;

        var sp = FindSpawn(_nextSpawnId) ?? FindSpawn(defaultSpawnId);
        if (sp)
        {
            PlayerSpawner.Instance.SpawnOrMoveTo(sp.transform);
            var player = PlayerSpawner.Instance.Player;
            OnPlayerSpawned?.Invoke(player);
        }

        var binder = FindFirstObjectByType<CinemachineBinder>();
        binder?.BindForActivePlayer();

        yield return null;
        yield return null;

        uiState.EnterGameplay();
        State = GameState.Gameplay;
        OnGameplayReady?.Invoke();

        loading?.Hide();
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
