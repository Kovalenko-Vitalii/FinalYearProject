using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneLoader : MonoBehaviour
{
    public static SceneLoader Instance { get; private set; }

    private string _currentContentScene;
    private AsyncOperation _activeLoadOp;
    private string _pendingSceneName;

    public float Progress { get; private set; } = 0f;
    public bool IsLoading => _activeLoadOp != null && !_activeLoadOp.isDone;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public Coroutine LoadContent(string sceneName)
    {
        _pendingSceneName = sceneName;
        Progress = 0f;

        _activeLoadOp = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        _activeLoadOp.allowSceneActivation = true;

        return StartCoroutine(FinishSwapRoutine(sceneName));
    }

    public AsyncOperation LoadContentAsync(string sceneName, bool allowSceneActivation)
    {
        _pendingSceneName = sceneName;
        Progress = 0f;

        _activeLoadOp = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        _activeLoadOp.allowSceneActivation = allowSceneActivation;

        StartCoroutine(FinishSwapRoutine(sceneName));
        return _activeLoadOp;
    }

    public void ActivateLoadedScene()
    {
        if (_activeLoadOp != null)
            _activeLoadOp.allowSceneActivation = true;
    }

    private IEnumerator FinishSwapRoutine(string sceneName)
    {
        while (_activeLoadOp != null && !_activeLoadOp.isDone)
        {
            float raw = _activeLoadOp.progress;
            Progress = Mathf.Clamp01(raw < 0.9f ? raw / 0.9f : 1f);
            yield return null;
        }

        var loadedScene = SceneManager.GetSceneByName(sceneName);
        if (loadedScene.IsValid())
            SceneManager.SetActiveScene(loadedScene);

        if (!string.IsNullOrEmpty(_currentContentScene) && _currentContentScene != sceneName)
        {
            yield return SceneManager.UnloadSceneAsync(_currentContentScene);
        }

        _currentContentScene = sceneName;

        Progress = 1f;
        _activeLoadOp = null;
        _pendingSceneName = null;
    }
}
