using UnityEngine;
using System;
using System.Collections;

public class GameplayOrchestrator : MonoBehaviour
{
    public static GameplayOrchestrator Instance { get; private set; }

    public event Action OnGameplayReady;

    [SerializeField] private UIStateController uiState;
    [SerializeField] private string menuScene = "MainMenu";
    [SerializeField] private string firstLevel = "Level01";
    [SerializeField] private string defaultSpawnId = "Start";

    [SerializeField] private string _nextSpawnId = "level01";

    public enum GameState { Boot, MainMenu, Loading, Gameplay }
    public GameState State { get; private set; } = GameState.Boot;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void EnterMenu()
    {
        State = GameState.MainMenu;
        uiState.EnterMainMenu();
        SceneLoader.Instance.LoadContent(menuScene);
        PlayerSpawner.Instance.Despawn();
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

        uiState.EnterGameplay();

        yield return SceneLoader.Instance.LoadContent(sceneName);

        var sp = FindSpawn(_nextSpawnId) ?? FindSpawn(defaultSpawnId);
        if (sp) PlayerSpawner.Instance.SpawnOrMoveTo(sp.transform);

        UnityEngine.Object.FindFirstObjectByType<CinemachineBinder>()?.BindForActivePlayer();

        State = GameState.Gameplay;
        OnGameplayReady?.Invoke();
    }

    private PlayerSpawnPoint FindSpawn(string id)
    {
        var points = UnityEngine.Object.FindObjectsByType<PlayerSpawnPoint>(FindObjectsSortMode.None);
        foreach (var p in points)
            if (p.Id == id) return p;
        return null;
    }
}
