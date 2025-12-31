using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneLoader : MonoBehaviour
{
    public static SceneLoader Instance { get; private set; }

    // Name of current loaded in content scene
    private string _currentContentScene;
    // Async operation for paralel loading
    private AsyncOperation _activeLoadOp;

    // Float that indicated loading progress, used mainly for UI
    public float Progress { get; private set; } = 0f;

    // Shows if scene loading at the moment
    public bool IsLoading => _activeLoadOp != null && !_activeLoadOp.isDone;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // Load content scene and activate automatically when loaded
    public Coroutine LoadContent(string sceneName)
    {
        // Resetting load progress
        Progress = 0f;

        // Adding loading process to async and activate it
        _activeLoadOp = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        
        _activeLoadOp.allowSceneActivation = true;

        return StartCoroutine(FinishSwapRoutine(sceneName));
    }

    // Save as LoadContent but user can choose when to finally load scene
    public AsyncOperation LoadContentAsync(string sceneName, bool allowSceneActivation)
    {
        Progress = 0f;

        _activeLoadOp = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        _activeLoadOp.allowSceneActivation = allowSceneActivation;

        StartCoroutine(FinishSwapRoutine(sceneName));
        return _activeLoadOp;
    }

    // Trigger scene activation
    public void ActivateLoadedScene()
    {
        if (_activeLoadOp != null)
            _activeLoadOp.allowSceneActivation = true;
    }

    // This method used to wait for scene load and than switch it with current one
    private IEnumerator FinishSwapRoutine(string sceneName)
    {
        // Waiting for loading and updating progress
        while (_activeLoadOp != null && !_activeLoadOp.isDone)
        {
            float raw = _activeLoadOp.progress;
            Progress = Mathf.Clamp01(raw < 0.9f ? raw / 0.9f : 1f);
            yield return null;
        }

        // Making this scene active
        var loadedScene = SceneManager.GetSceneByName(sceneName);
        if (loadedScene.IsValid())
            SceneManager.SetActiveScene(loadedScene);

        // Unloading previous scene
        if (!string.IsNullOrEmpty(_currentContentScene) && _currentContentScene != sceneName)
        {
            yield return SceneManager.UnloadSceneAsync(_currentContentScene);
        }

        // Setting loaded scene as current, updating progress
        _currentContentScene = sceneName;

        Progress = 1f;
        _activeLoadOp = null;
    }
}
